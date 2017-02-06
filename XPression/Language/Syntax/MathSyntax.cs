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
using XPression.Core;
using XPression.Core.Tokens;

namespace XPression.Language.Syntax
{
   public class MathSyntax<TExtender> : BaseSyntax<TExtender> where TExtender : new()
   {
      public static readonly MathSyntax<TExtender> Instance = new MathSyntax<TExtender>();

      protected MathSyntax() : base(false)
      {
         SyntaxChars.Add(new Dictionary<int, TokenType>
         {
            {'(',TokenType.LParen },
            {')',TokenType.RParen },
            {'|',TokenType.BitwiseOr },
            {'&',TokenType.BitwiseAnd },
            {'^',TokenType.BitwiseXOr },
            {'!',TokenType.LogicalNot},
            {'=',TokenType.Equal},
            {'<',TokenType.LessThan},
            {'>',TokenType.GreaterThan},
            {'~',TokenType.BitwiseNot},
            {'/',TokenType.Div},
            {'*',TokenType.Mul},
            {'+',TokenType.Add},
            {',',TokenType.Delimiter}
         });


         Constants.Add(new Dictionary<string, object>
         {
            {"PI",Math.PI },
            {"E",Math.E },
            {"null",null },
            {"true",true},
            {"false",false},
            {"NaN",double.NaN},
            {"INF",double.PositiveInfinity}
         });


         Symbols.Add(new Dictionary<string, TokenType>
         {
            {"-",TokenType.Sub }, 
            {"&&",TokenType.LogicalAnd },
            {"||",TokenType.LogicalOr },
            {"==",TokenType.Equal },
            {"!=",TokenType.NotEqual },
            {"<=",TokenType.LessThanOrEqual },
            {">=",TokenType.GreaterThanOrEqual },
            {">>",TokenType.RightShift },
            {"<<",TokenType.LeftShift }
         });

         this.AddMathFunctions();


         KnownTypes.Add(new Dictionary<string, TypeParser>
         {
            {"byte", new TypeParser(typeof (byte), lex => byte.Parse(lex))},
            {"decimal", new TypeParser(typeof (decimal), lex => decimal.Parse(lex))},
            {"double", new TypeParser(typeof (double), lex => double.Parse(lex))},
            {"float", new TypeParser(typeof (float), lex => float.Parse(lex))},
            {"int16", new TypeParser(typeof (Int16), lex => Int16.Parse(lex))},
            {"int32", new TypeParser(typeof (Int32), lex => int.Parse(lex))},
            {"int64", new TypeParser(typeof (Int64), lex => long.Parse(lex))},
            {"sbyte", new TypeParser(typeof (sbyte), lex => sbyte.Parse(lex))}
         });

         CompleteInitialization();

      }


   }
}
