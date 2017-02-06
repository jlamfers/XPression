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
using XPression.Core.Functions;
using XPression.Core.Tokens;

namespace XPression.Core
{
   public abstract class BaseGrammar : IGrammar
   {

      protected static readonly IDictionary<TokenType, OperatorInfo>
         Operators;

      static BaseGrammar()
      {
         var operators = new[]
         {

            new OperatorInfo(TokenType.LogicalOr, 10, Associativity.LeftToRight),
            new OperatorInfo(TokenType.LogicalAnd, 20, Associativity.LeftToRight),

            new OperatorInfo(TokenType.Assignment, 21, Associativity.RightToLeft),
            new OperatorInfo(TokenType.FunctionalBinaryOperator, 22, Associativity.RightToLeft),

            new OperatorInfo(TokenType.BitwiseOr, 30, Associativity.LeftToRight),
            new OperatorInfo(TokenType.BitwiseXOr, 40, Associativity.LeftToRight),
            new OperatorInfo(TokenType.BitwiseAnd, 50, Associativity.LeftToRight),

            new OperatorInfo(TokenType.Equal, 60, Associativity.LeftToRight),
            new OperatorInfo(TokenType.NotEqual, 60, Associativity.LeftToRight),

            new OperatorInfo(TokenType.GreaterThanOrEqual, 70, Associativity.LeftToRight),
            new OperatorInfo(TokenType.GreaterThan, 70, Associativity.LeftToRight),
            new OperatorInfo(TokenType.LessThanOrEqual, 70, Associativity.LeftToRight),
            new OperatorInfo(TokenType.LessThan, 70, Associativity.LeftToRight),

            new OperatorInfo(TokenType.LeftShift, 80, Associativity.LeftToRight),
            new OperatorInfo(TokenType.RightShift, 80, Associativity.LeftToRight),

            new OperatorInfo(TokenType.Sub, 90, Associativity.LeftToRight),
            new OperatorInfo(TokenType.Add, 90, Associativity.LeftToRight),

            new OperatorInfo(TokenType.Mul, 100, Associativity.LeftToRight),
            new OperatorInfo(TokenType.Div, 100, Associativity.LeftToRight),
            new OperatorInfo(TokenType.Mod, 100, Associativity.LeftToRight),

            new OperatorInfo(TokenType.BitwiseNot, 110, Associativity.RightToLeft),
            new OperatorInfo(TokenType.LogicalNot, 110, Associativity.RightToLeft),
            new OperatorInfo(TokenType.Negate, 110, Associativity.RightToLeft),

            new OperatorInfo(TokenType.Pow, 115, Associativity.LeftToRight),

            new OperatorInfo(TokenType.Contains, 120, Associativity.LeftToRight),
            new OperatorInfo(TokenType.StartsWith, 120, Associativity.LeftToRight),
            new OperatorInfo(TokenType.EndsWith, 120, Associativity.LeftToRight),
            new OperatorInfo(TokenType.Has, 120, Associativity.LeftToRight),
            new OperatorInfo(TokenType.Like, 120, Associativity.LeftToRight),

//            new OperatorInfo(TokenType.FunctionalBinaryOperator, 130, Associativity.RightToLeft),
            new OperatorInfo(TokenType.FunctionalUnaryOperator, 140, Associativity.RightToLeft),

            new OperatorInfo(TokenType.Array, 150, Associativity.LeftToRight),
            new OperatorInfo(TokenType.Function, 150, Associativity.LeftToRight),

            new OperatorInfo(TokenType.Declaration, 160, Associativity.RightToLeft),


         };

         Operators = operators.ToDictionary(x => x.TokenType, x => x).AsReadOnly();

      }

      protected BaseGrammar()
      {
         Strict = true;
      }
      
      public bool Strict { get; set; }

      public bool IsOperator(Token token)
      {
         return Operators.ContainsKey(token.Type);
      }

      public bool IsOperator(Token token, out OperatorInfo info)
      {
         return Operators.TryGetValue(token.Type, out info);
      }

      public OperatorInfo GetOperatorInfo(Token token)
      {
         return Operators[token.Type];
      }

      public virtual bool TryGetTypeParser(string typeName, out TypeParser typeParser)
      {
         typeParser = null;
         return false;
      }

      public virtual Type GetType(string typeName)
      {
         return Type.GetType(typeName, true, true);
      }

      public virtual IFunctionBuilder FunctionBuilder
      {
         get { return new FunctionFactory(Functions,StringComparer); } 
      }

      public virtual IList<FunctionMap> Functions
      {
         get { return new FunctionMap[0]; }
      }

      public abstract ILexer Lexer { get;  }
      public abstract bool ImplementsVariables { get; }
      public abstract IEqualityComparer<string> StringComparer { get; }
      public abstract bool IgnoreCase { get; }

      public abstract char IdentifierDelimiter { get; }
   }


   
}
