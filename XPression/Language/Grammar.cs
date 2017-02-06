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
using System.Collections.Concurrent;
using System.Collections.Generic;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Language.Syntax;

namespace XPression.Language
{
   public class Grammar : BaseGrammar
      
   {
      private static readonly ConcurrentDictionary<Type, IFunctionBuilder>
         FunctionsBuilders = new ConcurrentDictionary<Type, IFunctionBuilder>();

      private readonly ILexer 
         _lexer;

      public Grammar(ISyntax syntax)
      {
         if (syntax == null) throw new ArgumentNullException("syntax");
         Syntax = syntax;
         _lexer = new Lexer(this);
      }

      public ISyntax Syntax { get; private set; }

      public override bool TryGetTypeParser(string typeName, out TypeParser typeParser)
      {
         return Syntax.TryGetTypeParser(typeName, out typeParser);
      }

      public override Type GetType(string typeName)
      {
         return Syntax.GetType(typeName);
      }

      public override IFunctionBuilder FunctionBuilder
      {
         get { return FunctionsBuilders.GetOrAdd(Syntax.GetType(), t => new FunctionFactory(Syntax.Functions,StringComparer)); }
      }

      public override IList<FunctionMap> Functions
      {
         get { return Syntax.Functions; }
      }

      public override ILexer Lexer { get { return _lexer; } }

      public override bool ImplementsVariables
      {
         get { return Syntax.ImplementsVariables; }
      }

      public override IEqualityComparer<string> StringComparer
      {
         get { return Syntax.StringEqualityComparer; }
      }

      public override bool IgnoreCase
      {
         get { return Syntax.IgnoreCase; }
      }

      public override char IdentifierDelimiter
      {
         get { return Syntax.IdentifierDelimiter; }
      }
   }

   
}
