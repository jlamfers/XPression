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

using XPression.Core.Tokens;

namespace XPression.Core
{
   public class OperatorInfo
   {
      public OperatorInfo(TokenType tokenType, int precedence, Associativity associativity)
      {
         Associativity = associativity;
         Precedence = precedence;
         TokenType = tokenType;
      }

      public TokenType TokenType { get; private set; }
      public int Precedence { get; private set; }
      public Associativity Associativity { get; private set; }
   }
}