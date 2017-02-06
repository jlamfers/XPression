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

namespace XPression.Core.Tokens
{
   public static class TokenExtension
   {

      public static bool IsIdentifierOrLiteral(this  Token token)
      {
         return token.Type == TokenType.Identifier || token.Type == TokenType.Literal;
      }

      public static bool IsFunctionalUnaryOperator(this Token token)
      {
         return token.Type == TokenType.FunctionalUnaryOperator;
      }
      public static bool IsFunctionOrArray(this Token token)
      {
         return token != null && token.Type == TokenType.Function || token.Type == TokenType.Array;
      }
      public static bool IsFunction(this Token token)
      {
         return token != null && token.Type == TokenType.Function;
      }
      public static bool IsArray(this Token token)
      {
         return token != null && token.Type == TokenType.Array;
      }
      public static bool IsLiteral(this Token token)
      {
         return token.Type == TokenType.Literal;
      }
      public static bool IsLParen(this Token token)
      {
         return token.Type == TokenType.LParen;
      }
      public static bool IsRParen(this Token token)
      {
         return token.Type == TokenType.RParen;
      }
      public static bool IsLBracket(this Token token)
      {
         return token.Type == TokenType.LArrayBracket;
      }
      public static bool IsRBracket(this Token token)
      {
         return token.Type == TokenType.RArrayBracket;
      }
      public static bool IsDelimiter(this Token token)
      {
         return token.Type == TokenType.Delimiter;
      }
      public static bool IsIdentifierDelimiter(this Token token)
      {
         return token.Type == TokenType.IdentifierDelimiter;
      }
      public static bool IsNull(this Token token)
      {
         return token.IsLiteral() && ((LiteralToken) token).ConvertedValue == null;
      }
      public static bool IsNegate(this Token token)
      {
         return token.Type == TokenType.Negate;
      }
      public static bool IsString(this Token token)
      {
         return token.Type == TokenType.Literal && ((LiteralToken) token).ConvertedValue is string; 
      }
   }
}