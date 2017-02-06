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
using XPression.Core.Functions;
using XPression.Core.Tokens;

namespace XPression.Core
{
   public interface IGrammar
   {
      bool IsOperator(Token token);
      bool IsOperator(Token token, out OperatorInfo info);
      OperatorInfo GetOperatorInfo(Token token);

      bool Strict { get; set; }

      bool TryGetTypeParser(string typeName, out TypeParser typeParser);
      Type GetType(string typeName);

      IFunctionBuilder FunctionBuilder { get; }
      ILexer Lexer { get; }
      IList<FunctionMap> Functions { get; }
      bool ImplementsVariables { get; }
      IEqualityComparer<string> StringComparer { get; }
      bool IgnoreCase { get; }
      char IdentifierDelimiter { get; }
   }
}