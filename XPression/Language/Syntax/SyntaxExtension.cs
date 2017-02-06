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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using XPression.Core;
using XPression.Core.Functions;

namespace XPression.Language.Syntax
{
   public static class SyntaxExtension
   {
      public static Type GetType(this ISyntax self, string typename)
      {
         TypeParser type;
         return self.KnownTypes.TryGetValue(typename, out type)
            ? type.Type
            : TypeResolver.GetType(typename, true, false);
      }

      public static bool TryGetTypeParser(this ISyntax self, string typename, out TypeParser typeparser)
      {
         if (self.KnownTypes.TryGetValue(typename, out typeparser))
         {
            return true;
         }
         var type = TypeResolver.GetType(typename, false, self.IgnoreCase); 
         if (type != null)
         {
            var c = TypeDescriptor.GetConverter(type);
            if (c.CanConvertFrom(typeof (string)))
            {
               typeparser = new TypeParser(type, c.ConvertFrom);
               return true;
            }
         }
         return false;
      }

      public static bool IsSyntaxChar(this ISyntax self, int chr)
      {
         return self.SyntaxChars.ContainsKey(chr);
      }

      public static bool IsLParen(this ISyntax self, int chr)
      {
         //trivial
         return chr == '(';
      }
      public static bool IsRParen(this ISyntax self, int chr)
      {
         //trivial
         return chr == ')';
      }
      public static bool IsParen(this ISyntax self, int chr)
      {
         //trivial
         return chr == '(' || chr == ')';
      }

      public static bool TryAddSqlServerSpatialTypes(this ISyntax self)
      {
         // NOTE  I: you need to install package "Microsoft.SqlServer.Types (spatial)" in order to let the following functions work
         // these bindings do NOT work with entity framework (at least not with all versions)
         // you need to reference XPression.EntityFramework in order to let these functions interact with EF from version EF5/.NET 4 and higher
         var result = false;
         var geo = Type.GetType("Microsoft.SqlServer.Types.SqlGeography, Microsoft.SqlServer.Types");
         if (geo != null)
         {
            result = true;
            self.Functions.Add(new FunctionMap("geo.distance", geo.GetMethod("STDistance")));
            self.Functions.Add(new FunctionMap("geo.length", geo.GetMethod("STLength")));
            self.Functions.Add(new FunctionMap("geo.intersects", geo.GetMethod("STIntersects")));
         }

         geo = Type.GetType("Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types");
         if (geo != null)
         {
            result = true;
            self.Functions.Add(new FunctionMap("geo.distance", geo.GetMethod("STDistance")));
            self.Functions.Add(new FunctionMap("geo.length", geo.GetMethod("STLength")));
            self.Functions.Add(new FunctionMap("geo.intersects", geo.GetMethod("STIntersects")));
         }

         return result;
      }

      public static void AddMathFunctions(this ISyntax self)
      {
         self.Functions.Add(typeof (Math)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Select(m => new FunctionMap(m.Name.ToLower(), m)));
      }

   }
}
