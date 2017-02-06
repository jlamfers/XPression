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
using XPression.Core;
using XPression.Core.Functions;
using XPression.Core.Tokens;

namespace XPression.Language.Syntax
{
   public abstract class BaseSyntax<TExtender> : ISyntax
      where TExtender: new()
   {
      private bool _isReadOnly;

      protected BaseSyntax(bool ignoreCase)
      {
         IgnoreCase = ignoreCase;
         StringEqualityComparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
         Quote = '\'';
         LineComment = '\0';
         SyntaxChars = new Dictionary<int, TokenType>();
         Symbols = new Dictionary<string, TokenType>(StringEqualityComparer);
         Constants = new Dictionary<string, object>(StringEqualityComparer);
         Functions = new List<FunctionMap>();
         KnownTypes = new Dictionary<string, TypeParser>(StringEqualityComparer);
         NonBreakingIdentifierChars = new List<char>();
      }

      // this method must be invoked in any subclass immediatly after constructor completion
      protected void CompleteInitialization()
      {
         lock (this)
         {
            if (_isReadOnly)
            {
               return;
            }
            _isReadOnly = true;
         }

         if (typeof(ISyntaxExtender).IsAssignableFrom(typeof(TExtender)))
         {
            var api = new TExtender().CastTo<ISyntaxExtender>();
            api.ExtendSyntax(this);
         }

         NonBreakingIdentifierChars = NonBreakingIdentifierChars.CastTo<List<char>>().AsReadOnly();
         SyntaxChars = SyntaxChars.CastTo<Dictionary<int, TokenType>>().AsReadOnly();
         Constants = Constants.CastTo<Dictionary<string, object>>().AsReadOnly();
         Symbols = Symbols.CastTo<Dictionary<string, TokenType>>().AsReadOnly();
         Functions = Functions.CastTo<List<FunctionMap>>().AsReadOnly();
         KnownTypes = KnownTypes.CastTo<Dictionary<string, TypeParser>>().AsReadOnly();
         ImplementsVariables = Symbols.Values.Any(v => v == TokenType.Declaration) || SyntaxChars.Values.Any(v => v == TokenType.Declaration);
         IdentifierDelimiter = (char)SyntaxChars.Where(kv => kv.Value == TokenType.IdentifierDelimiter).Select(kv => kv.Key).FirstOrDefault();
      }



      public bool IgnoreCase { get; private set; }

      public IEqualityComparer<string> StringEqualityComparer { get; private set; }

      public char Quote { get; protected set; }

      public char IdentifierDelimiter { get; protected set; }

      public char LineComment { get; protected set; }

      public bool ImplementsVariables { get; private set; }

      public IDictionary<int, TokenType> SyntaxChars { get; private set; }

      public IDictionary<string, TokenType> Symbols { get; private set; }

      public IDictionary<string, object> Constants { get; private set; }

      public IList<FunctionMap> Functions { get; private set; }

      public IDictionary<string, TypeParser> KnownTypes { get; private set; }

      public IList<char> NonBreakingIdentifierChars { get; private set; }

   }
}
