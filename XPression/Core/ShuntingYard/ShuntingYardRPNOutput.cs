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

using System.Collections;
using System.Collections.Generic;
using XPression.Core.Tokens;

namespace XPression.Core.ShuntingYard
{
   public class ShuntingYardRPNOutput : IShuntingYardOutput
   {
      private readonly List<Token> _list = new List<Token>(256);
      private string _source;

      public void Add(Token token, bool isOperator = false)
      {
         _source = _source ?? token.Source;
         _list.Add(token);
      }

      public IEnumerator<Token> GetEnumerator()
      {
         return _list.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public List<Token> ToList()
      {
         return _list;
      } 
   }
}