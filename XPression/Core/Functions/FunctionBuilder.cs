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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace XPression.Core.Functions
{
   public class FunctionBuilder
   {

      public FunctionBuilder(MemberInfo member, FunctionSignature key)
      {
         Member = member;
         var parameterTypes = new List<Type>();
         IsStatic = member.IsStatic();
         if (!IsStatic)
         {
            // handle declaring type as first parameter
            parameterTypes.Add(member.DeclaringType ?? member.ReflectedType);
         }
         if (Method != null)
         {
            // it is a method, else it is a property or field
            parameterTypes.AddRange(Method.GetParameters().Select(p => p.ParameterType));
         }
         // else it is a field or property

         ParameterTypes = parameterTypes.AsReadOnly();
         Key = key;
         NeedTypeCast = !Key.SequenceEqual(parameterTypes);
         HasDelegateArg = key.Any(t => typeof(Delegate).IsAssignableFrom(t));

      }

      public bool IsStatic { get; private set; }
      public bool NeedTypeCast { get; private set; }
      public bool HasDelegateArg { get; private set; }
      public MemberInfo Member { get; private set; }
      public MethodInfo Method { get { return Member as MethodInfo; } }
      public IList<Type> ParameterTypes { get; private set; }
      public FunctionSignature Key { get; private set; }

      public Expression BuildExpression(List<Expression> functionParameters, ParameterExpression p)
      {
         if (NeedTypeCast || HasDelegateArg)
         {
            if (HasDelegateArg)
            {
               var args = new List<Expression>();
               for (var i = 0; i < functionParameters.Count; i++)
               {
                  if (typeof (Delegate).IsAssignableFrom(ParameterTypes[i]))
                  {
                     var action = Expression.Lambda(functionParameters[i], p).Compile();
                     args.Add(Expression.Constant(action).Convert<Delegate>());
                  }
                  else
                  {
                     args.Add(functionParameters[i].Convert(ParameterTypes[i]));
                  }
               }
               functionParameters = args;
            }
            else
            {
               functionParameters = functionParameters.Select((t, i) => t.Convert(ParameterTypes[i])).ToList();
            }
         }

         if (Method != null)
         {
            return IsStatic
               ? Expression.Call(Method, functionParameters.ToArray())
               : Expression.Call(functionParameters.First(), Method, functionParameters.Skip(1).ToArray());
         }

         return Expression.MakeMemberAccess(functionParameters.FirstOrDefault(), Member);
      }
   }
}