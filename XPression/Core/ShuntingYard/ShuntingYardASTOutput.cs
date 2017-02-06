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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XPression.Core.Tokens;

namespace XPression.Core.ShuntingYard
{
   public class ShuntingYardASTOutput  : IShuntingYardOutput
   {
      private readonly ParameterExpression _p;
      private readonly Stack<Token> _stack = new Stack<Token>(64);
      private readonly ASTBuilder _astBuilder;
      private string _source;

      public ShuntingYardASTOutput(ASTBuilder astBuilder, ParameterExpression p)
      {
         _p = p;
         _astBuilder = astBuilder;
      }

      public void Add(Token token, bool isOperator = false)
      {
         _source = _source ?? token.Source;

         ///////////////////////////////
         // SCRIPTING 
         if (isOperator && _stack.Count < 2 && token.Type == TokenType.LogicalAnd && token.Lexeme == ";")
         {
            // most probably script char that represents empty statement
            // => ignore
            return;
         }
         //
         //////////////////////////////

         _stack.Push(token);
         if (!isOperator) return;

         try
         {
            var ast = _astBuilder.PopExpression(_stack, _p);
            _stack.Push(ast.ToToken());
         }
         catch (XPressionException)
         {
            throw;
         }
         catch (Exception ex)
         {
            throw new XPressionException(token.Source,ex.Message,token.Position,ex);
         }
      }

      public Expression GetResultAST()
      {
         if (_stack.Count > 1)
         {
            throw new XPressionException(_source, "missing operator", (_stack.FirstOrDefault(t => !(t is ExpressionToken)) ?? _stack.First()).Position);
         }

         if (_stack.Count == 0)
         {
            throw new XPressionException(_source, "operator error", 0);
         }

         return _astBuilder.PopExpression(_stack, _p);
      }

      public IEnumerator<Token> GetEnumerator()
      {
         return _stack.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}