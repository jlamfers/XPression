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
   public class LiteralToken : Token
   {
      public LiteralToken()
      {
         Type = TokenType.Literal;
      }
      public string TypeName { get; set; }
      public object ConvertedValue { get; set; }

      public static LiteralToken CreateLiteral(string source, string lexeme, int position, object convertedValue)
      {
         return new LiteralToken
         {
            Source = source,
            Lexeme = lexeme,
            Position = position,
            Type = TokenType.Literal,
            ConvertedValue = convertedValue
         };
      }
   }
}
