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

namespace XPression.Language.Syntax
{
   public class DefaultSyntaxExtender : ISyntaxExtender
   {
      // singletons collection in base class
      private static readonly IDictionary<Type,ISyntax> 
         _syntax = new Dictionary<Type, ISyntax>(); 

      public virtual void ExtendSyntax(ISyntax syntax)
      {
         _syntax[GetType()] = syntax;

         foreach (var type in AppDomain.CurrentDomain.GetExportedTypes().Where(t => !t.IsAbstract && typeof(IAutoSyntaxExtender).IsAssignableFrom(t)))
         {
            Activator.CreateInstance(type).CastTo<ISyntaxExtender>().ExtendSyntax(syntax);

         }
      }

      public virtual ISyntax Syntax
      {
         get { return _syntax[GetType()]; }
      }
   }
}
