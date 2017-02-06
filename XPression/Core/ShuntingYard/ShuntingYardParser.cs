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
using XPression.Core.Tokens;

namespace XPression.Core.ShuntingYard
{
   // see Shunting-yard algorithm
   // https://en.wikipedia.org/wiki/Shunting-yard_algorithm


   public class ShuntingYardParser
   {

      private readonly IGrammar _grammar;
      private readonly ASTBuilder _astBuilder;

      public ShuntingYardParser(IGrammar grammar)
      {
         if (grammar == null) throw new ArgumentNullException("grammar");
         _grammar = grammar;
         _astBuilder = new ASTBuilder(grammar);
      }

      public void Parse(IEnumerable<Token> input, IShuntingYardOutput output)
      {
         if (input == null) throw new ArgumentNullException("input");
         if (output == null) throw new ArgumentNullException("output");

         var tokens = input as Token[] ?? input.ToArray();
         var source = tokens.Select(i => i.Source).FirstOrDefault();

         var operatorStack = new Stack<Token>();

         foreach (var token in tokens)
         {
            if (token.IsIdentifierOrLiteral())
            {
               output.Add(token);
               continue;
            }

            if (token.IsFunctionOrArray())
            {
               operatorStack.Push(token);
               continue;
            }

            Token top;
            if (token.IsDelimiter())
            {
               while (operatorStack.Any())
               {
                  top = operatorStack.First();
                  if (!top.IsLParen() && !top.IsLBracket())
                  {
                     output.Add(operatorStack.Pop(), true);

                  }
                  else
                  {
                     break;
                  }
               }

               if (!operatorStack.Any())
               {
                  throw new XPressionException(source, "parenthesis mismatch: missing '(' with function call", token.Position);
               }
               continue;
            }

            OperatorInfo opr;
            if (_grammar.IsOperator(token, out opr)) 
            {
               var precedence = opr.Precedence;
               var isLeftAssociative = opr.Associativity == Associativity.LeftToRight;

               while (operatorStack.Any())
               {
                  if (!_grammar.IsOperator(operatorStack.First(), out opr)) 
                  {
                     break;
                  }
                  if (((isLeftAssociative && (precedence <= opr.Precedence)) || (!isLeftAssociative) && (precedence < opr.Precedence)))
                  {
                     output.Add(operatorStack.Pop(), true);
                  }
                  else
                  {
                     break;
                  }
               }
               operatorStack.Push(token);
               continue;
            }

            if (token.IsLParen() || token.IsLBracket())
            {
               operatorStack.Push(token);
               continue;
            }

            if (token.IsRParen() || token.IsRBracket())
            {
               var lparen = false;

               while (operatorStack.Any())
               {

                  top = operatorStack.First();
                  if (top.IsLParen() || top.IsLBracket())
                  {
                     lparen = true;
                     break;
                  }
                  output.Add(operatorStack.Pop(), true);
               }

               if (!lparen)
               {
                  throw new XPressionException(source, "parenthesis mismatch, missing '('", token.Position);
               }

               operatorStack.Pop();

               top = operatorStack.FirstOrDefault();

               if (top != null && top.IsFunctionOrArray())
               {
                  output.Add(operatorStack.Pop(), true);
               }
            }
         }

         while (operatorStack.Any())
         {
            var first = operatorStack.First();
            if (first.IsLParen() || first.IsRParen() || first.IsLBracket() || first.IsRBracket())
            {
               throw new XPressionException(source, "invalid parenthese: " + first.Lexeme, first.Position);
            }
            output.Add(operatorStack.Pop(), true);
         }

      }

      public IList<Token> BuildRPN(IEnumerable<Token> input)
      {
         var output = new ShuntingYardRPNOutput();
         Parse(input, output);
         return output.ToList();
      }

      public Expression<Func<T, TResult>> BuildAST<T, TResult>(IEnumerable<Token> input)
      {
         var p = Expression.Parameter(typeof(T), "p");
         var output = new ShuntingYardASTOutput(_astBuilder, p);
         Parse(input, output);

         var ast = output.GetResultAST();

         if (typeof(TResult).IsValueType && !typeof(TResult).IsNullable() && ast.IsNullConstant())
         {
            ast = Expression.Convert(ast, typeof(Nullable<>).MakeGenericType(typeof(TResult)));
         }

         if (typeof(TResult).EnsureNotNullable() == ast.Type.EnsureNotNullable())
         {
            if (typeof(TResult).IsValueType && ast.Type.IsNullable() && !typeof(TResult).IsNullable())
            {
               ast = Expression.Call(ast, ast.Type.GetMethod("GetValueOrDefault", Type.EmptyTypes));
            }
         }
         else
         {
            ast = Expression.Convert(ast, typeof(TResult));
         }

         return Expression.Lambda<Func<T, TResult>>(ast, p);
      }
   }
}
