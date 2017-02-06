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
using System.Linq.Expressions;
using XPression.Core.ShuntingYard;

namespace XPression.Core
{
   /// <summary>
   /// This class parses simple expressions with configurable syntax
   /// </summary>
   public class Parser
      
   {
      public class TypedParser<TResult>
      {
         private readonly Parser _parser;

         public TypedParser(Parser parser)
         {
            _parser = parser;
         }

         public Expression<Func<T, TResult>> Parse<T>(string expression, T input)
         {
            return _parser.Parse<T, TResult>(expression);
         }
         public Func<T, TResult> Compile<T>(string expression, T input)
         {
            return _parser.Compile<T, TResult>(expression);
         }
      }


      private readonly ShuntingYardParser
         _shuntingYard;

      /// <summary>
      /// Constructor that requires a lexer
      /// </summary>
      /// <param name="grammar">the grammar</param>
      public Parser(IGrammar grammar)
      {
         if (grammar == null) throw new ArgumentNullException("grammar");
         Grammar = grammar;
         _shuntingYard = new ShuntingYardParser(grammar);
      }

      /// <summary>
      /// Gets the grammar
      /// </summary>
      public IGrammar Grammar { get; private set; }

      /// <summary>
      /// Parses the source, and returns an expression tree
      /// </summary>
      /// <typeparam name="T">The input parameter type</typeparam>
      /// <typeparam name="TResult">The return type</typeparam>
      /// <param name="expression">The expression that is parsed</param>
      /// <returns></returns>
      public Expression<Func<T, TResult>> Parse<T,TResult>(string expression)
      {
         if (expression == null) throw new ArgumentNullException("expression");

         var tokens = Grammar.Lexer.Tokenize(expression);

         return _shuntingYard.BuildAST<T, TResult>(tokens);
      }
      public Expression<Func<T, object>> Parse<T>(string expression)
      {
         return Parse<T, object>(expression);
      }
      public Expression<Func<T, object>> Parse<T>(string expression, T input)
      {
         return Parse<T, object>(expression);
      }

      public Expression<Func<T, bool>> ParsePredicate<T>(string expression)
      {
         return Parse<T, bool>(expression);
      }
      public Expression<Func<T, bool>> ParsePredicate<T>(string expression, T input)
      {
         return Parse<T, bool>(expression);
      }

      public Func<T, TResult> Compile<T, TResult>(string expression)
      {
         return Parse<T, TResult>(expression).Compile();
      }
      public Func<T, object> Compile<T>(string expression, T input)
      {
         return Parse<T, object>(expression).Compile();
      }

      public Func<object> Compile(string expression)
      {
         var e = Parse<object, object>(expression);
         var f = e.Compile();
         return () => f(null);
      }
      public Func<TResult> Compile<TResult>(string expression)
      {
         var e = Parse<object, TResult>(expression);
         var f = e.Compile();
         return () => f(null);
      }

      public Func<T, bool> CompilePredicate<T>(string expression)
      {
         return ParsePredicate<T>(expression).Compile();
      }
      public Func<T, bool> CompilePredicate<T>(string expression, T input)
      {
         return ParsePredicate<T>(expression).Compile();
      }

      public TypedParser<TResult> WithReturnType<TResult>()
      {
         return new TypedParser<TResult>(this);
      }
   }

}
