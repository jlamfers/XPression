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

using System.Collections.Generic;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Core.Tokens;

namespace XPression.Language.Syntax
{
   public interface ISyntax
   {
      char Quote { get; }
      char IdentifierDelimiter { get; }
      char LineComment { get; }
      bool ImplementsVariables { get; }
      IEqualityComparer<string> StringEqualityComparer { get; }
      bool IgnoreCase { get; }



      /// <summary>
      /// SyntaxChars are char->TokenType mappings
      /// SyntaxChars are single chars with a significant meaning,
      /// like ,()=! etc
      /// </summary>
      IDictionary<int, TokenType> SyntaxChars { get; }

      /// <summary>
      /// Symbols are string->TokenType mappings
      /// Symbols are strings/identifiers with a significant meaning,
      /// like "not", "!=", etc
      /// </summary>
      IDictionary<string, TokenType> Symbols { get; }

      /// <summary>
      /// Constants are name->value mappings
      /// Constants are identifiers that represent constant literals,
      /// like "true", "false", "null", "NaN", etc
      /// </summary>
      IDictionary<string, object> Constants { get; }

      /// <summary>
      /// Functions are name->method mappings 
      /// </summary>
      IList<FunctionMap> Functions { get; }

      /// <summary>
      /// KnownTypes are name->type mappings 
      /// </summary>
      IDictionary<string, TypeParser> KnownTypes { get; }

      /// <summary>
      /// NonBreakingIdentifierChars contains chars that will
      /// not break any identifier tokenizing. By default any
      /// identifier starts with a letter/'_', followed by letter/'_'/digit
      /// example: 
      /// normally a dot '.' will break an identifier tokenizing
      /// now with NonBreakingIdentifierChars = new []{'.'};
      /// "Edm.String" is parsed as a whole identifier, so 
      /// is handled as one identifier token
      /// </summary>
      IList<char> NonBreakingIdentifierChars { get; }
   }
}