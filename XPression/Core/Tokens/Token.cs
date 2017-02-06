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

using System.Runtime.Remoting;

namespace XPression.Core.Tokens
{
   public class Token
   {
      public TokenType Type { get; set; }
      public virtual string Lexeme { get; set; }
      public int Position { get; set; }
      public string Source { get; set; }

      public static Token Create(TokenType type, string source, string lex, int position)
      {
         if (type == TokenType.FunctionalBinaryOperator || type == TokenType.FunctionalUnaryOperator)
         {
            return new FunctionToken
            {
               Type = type,
               Source = source,
               Lexeme = lex,
               Position = position,
               ParameterCount = type == TokenType.FunctionalBinaryOperator ? 2 : 1
            };
         }
         //if (type == TokenType.Function)
         //{
         //   return new FunctionToken
         //   {
         //      Source = source,
         //      Lexeme = lex,
         //      Position = position,
         //      ParameterCount = 1
         //   };
         //}
         return new Token
         {
            Type = type,
            Source = source,
            Lexeme = lex,
            Position = position,
         };
      }


      public override string ToString()
      {
         return string.Format("{0} : {1}", Type, Lexeme);
      }
   }
}