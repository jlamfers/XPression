#region  License
/*
Copyright 2017 - Jaap Lamfers - jlamfers@xipton.net

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 * */
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XPression.Core.Tokens;

namespace XPression.Core.Functions
{
   public class FunctionFactory : IFunctionBuilder
   {
      private readonly Dictionary<string, ConcurrentDictionary<FunctionSignature, FunctionBuilder>>
         _members;

      private readonly IDictionary<string, IFunctionBuilder>
         _builders;


      public FunctionFactory(IEnumerable<FunctionMap> map, IEqualityComparer<string> stringComparer)
      {
         _members = new Dictionary<string, ConcurrentDictionary<FunctionSignature, FunctionBuilder>>(stringComparer);
         _builders = new Dictionary<string, IFunctionBuilder>(stringComparer);
         var callMaps = map as FunctionMap[] ?? map.ToArray();
         if (callMaps.Length == 0) return;

         _members = new Dictionary<string, ConcurrentDictionary<FunctionSignature, FunctionBuilder>>(StringComparer.InvariantCultureIgnoreCase);
         foreach (var name in callMaps.Where(m => m.Member != null).Select(m => m.Name).Distinct(StringComparer.InvariantCultureIgnoreCase))
         {
            _members.Add(name, new ConcurrentDictionary<FunctionSignature, FunctionBuilder>());
         }
         foreach (var m in callMaps.Where(m => m.Member != null))
         {
            var signature = GetSignature(m.Member);
            _members[m.Name][signature] = new FunctionBuilder(m.Member, signature);
         }
         foreach (var m in callMaps.Where(m => m.Builder != null))
         {
            _builders[m.Name] = m.Builder;
         }
      }

      public bool TryBuildExpression(ASTBuilder astBuilder, FunctionToken functionToken, Stack<Token> stack, ParameterExpression p, out Expression output)
      {

         output = null;

         IFunctionBuilder builder;
         if (_builders.TryGetValue(functionToken.Lexeme, out builder))
         {
            if (builder.TryBuildExpression(astBuilder, functionToken, stack, p, out output))
            {
               return true;
            }
         }


         ConcurrentDictionary<FunctionSignature, FunctionBuilder> lookup;
         if (!_members.TryGetValue(functionToken.Lexeme, out lookup))
         {
            return false;
         }

         var args = new List<Expression>();

         for (var i = 0; i < functionToken.ParameterCount; i++)
         {
            args.Add(astBuilder.PopExpression(stack, p));
         }

         args.Add(p); // always initially probe instance call

         args.Reverse(); // reverse stack order

         FunctionBuilder factory;
         FunctionSignature 
            fs1 = null, // instance
            fs2 = null; // static
         if (!TryFindFactory(lookup, fs1 = new FunctionSignature(args.Select(fp => fp.Type)), out factory))
         {
            if (TryFindFactory(lookup, fs2 = new FunctionSignature(args.Skip(1).Select(fp => fp.Type)), out factory))
            {
               args = args.Skip(1).ToList();
            }
         }

         if (factory != null)
         {
            // found
            if (args.Any() && factory.ParameterTypes.Any())
            {
               if (factory.ParameterTypes.Last().IsArray && !args.Last().Type.IsArray)
               {
                  // we need an argument conversion to params <some-type>[]
                  if (fs2 != null)
                  {
                     // static invocation
                     if (factory.ParameterTypes[0] == typeof (object[]))
                     {
                        args = new List<Expression>(new[]{Expression.NewArrayInit(typeof (object), args.Select(a => a.Convert<object>()))});
                     }
                     else
                     {
                        args = new List<Expression>(new[] { Expression.NewArrayInit(args[0].Type, args)});
                     }
                  }
                  else
                  {
                     // instane invocation
                     if (factory.ParameterTypes[1] == typeof(object[]))
                     {
                        args = new List<Expression>(new[] { args[0], Expression.NewArrayInit(typeof(object), args.Skip(1).Select(a => a.Convert<object>())) });
                     }
                     else
                     {
                        args = new List<Expression>(new[] { args[0], Expression.NewArrayInit(args[0].Type, args.Skip(1)) });
                     }
                     
                  }
                  
               }
            }
            output = factory.BuildExpression(args, p);
            return true;
         }

         FunctionSignature signature;
         /////////////////////////////////////////////////////////////////////////////////////////////////////////
         // 1. try instance
         if (args.Count == 2 || args.Skip(1).Select(x => x.Type).Distinct().Count() == 1)
         {
            signature = new FunctionSignature(new[] { args[0].Type, args[1].Type.MakeArrayType() });
            if (TryFindFactory(lookup, signature, out factory))
            {
               // probably params <type>[] arg
               args = new List<Expression>
               {
                  args[0],
                  Expression.NewArrayInit(args[1].Type, args.Skip(1))
               };
               lookup.TryAdd(fs1, factory);// register in lookup
               output = factory.BuildExpression(args, p);
               return true;
            }            
         }

         // any delegates?
         var args1 = args;
         var factories = lookup.Values.Where(f => f.HasDelegateArg && f.ParameterTypes.Count == args1.Count);
         foreach (var f in factories)
         {
            var ismatch = true;
            var i = 0;
            foreach (var t in f.ParameterTypes)
            {
               if (!(typeof(Delegate).IsAssignableFrom(t) || t.IsAssignableFrom(args[i].Type)))
               {
                  ismatch = false;
                  break;
               }
               i++;
            }
            if (ismatch)
            {
               lookup.TryAdd(fs1, f);// register in lookup
               output = f.BuildExpression(args, p);
               return true;
            }
         }

         // params object[] ?
         signature = new FunctionSignature(new[] { args[0].Type, typeof(object).MakeArrayType() });
         if (TryFindFactory(lookup, signature, out factory))
         {
            // probably params object[] arg
            args = new List<Expression>
            {
               args[0],
               Expression.NewArrayInit(typeof (object), args.Skip(1).Select(e => e.Convert<object>()))
            };
            lookup.TryAdd(fs1, factory);// register in lookup
            output = factory.BuildExpression(args, p);
            return true;
         }
         //
         //////////////////////////////////////////////////////////////////////////////////////////////////////
         
         /////////////////////////////////////////////////////////////////////////////////////////////////////
         // 2. try static
         args = args.Skip(1).ToList();

         if (args.Count == 1 || args.Select(x => x.Type).Distinct().Count() == 1)
         {
            // all argumens have same type, look for array type
            signature = new FunctionSignature(new[] { args[0].Type.MakeArrayType() });
            if (TryFindFactory(lookup, signature, out factory))
            {
               // probably params <type>[] arg
               args = new List<Expression>
               {
                  Expression.NewArrayInit(args[0].Type, args)
               };
               lookup.TryAdd(fs2, factory);// register in lookup
               output = factory.BuildExpression(args, p);
               return true;
            }
         }

         // any delegate?
         var args2 = args;
         factories = lookup.Values.Where(f => f.HasDelegateArg && f.ParameterTypes.Count - 1 == args2.Count);
         foreach (var f in factories)
         {
            var ismatch = true;
            var i = 0;
            foreach (var t in f.ParameterTypes.Skip(1))
            {
               if (!(typeof(Delegate).IsAssignableFrom(t) || t.IsAssignableFrom(args[i].Type)))
               {
                  ismatch = false;
                  break;
               }
               i++;
            }
            if (ismatch)
            {
               lookup.TryAdd(fs2, f);// register in lookup
               output = f.BuildExpression(args, p);
               return true;
            }
         }

         // params object[] ?
         signature = new FunctionSignature(new[] { typeof(object).MakeArrayType() });
         if (TryFindFactory(lookup, signature, out factory))
         {
            // probably params object[] arg
            args = new List<Expression>
            {
               args[0],
               Expression.NewArrayInit(typeof(object), args.Skip(1).Select(e => e.Convert<object>()))
            };
            lookup.TryAdd(fs2, factory);// register in lookup
            output = factory.BuildExpression(args, p);
            return true;
         }
         throw new XPressionException(functionToken.Source, "invalid arguments, overload not found for method " + functionToken.Lexeme,functionToken.Position);

      }

      private static bool TryFindFactory(ConcurrentDictionary<FunctionSignature, FunctionBuilder> lookup, FunctionSignature signature, out FunctionBuilder factory)
      {
         if (lookup.TryGetValue(signature, out factory))
         {
            return true;
         }
         var overload = lookup.Keys.FirstOrDefault(s => s.IsInvokableBy(signature));
         if (overload != null)
         {
            factory = new FunctionBuilder(lookup[overload].Member, signature);
            lookup.TryAdd(signature, factory);
            return true;
         }
         return false;
      }

      private static FunctionSignature GetSignature(MemberInfo member)
      {
         var parameterTypes = new List<Type>();
         var isStatic = member.IsStatic();
         if (!isStatic)
         {
            parameterTypes.Add(member.DeclaringType ?? member.ReflectedType);
         }
         var method = member as MethodInfo;
         if (method != null)
         {
            parameterTypes.AddRange(method.GetParameters().Select(p => p.ParameterType));
         }
         return new FunctionSignature(parameterTypes);
      }
   }
}