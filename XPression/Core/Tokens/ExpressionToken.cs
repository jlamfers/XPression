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

using System.Linq.Expressions;

namespace XPression.Core.Tokens
{
   /// <summary>
   /// an expression token is a token that holds a compiled expression sub tree. 
   /// an expression token can be pushed "back" on the stack so that it can 
   /// be popped again, like any other token, by the astbuilder, as requested 
   /// by further operations
   /// </summary>
   public class ExpressionToken : Token
   {
      public ExpressionToken()
      {
         Type = TokenType.Expression;
      }

      public Expression Expression { get; set; }
   }
}