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
using System.Linq;

namespace XPression.Core
{
   public static class TypeResolver
   {
      private static readonly ConcurrentDictionary<string, Type>
         Types = new ConcurrentDictionary<string, Type>(),
         TypesIgnoreCase = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

      public static Type GetType(string typename, bool throwOnError, bool ignorecCase)
      {
         var comparer = ignorecCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
         var cache = ignorecCase ? TypesIgnoreCase : Types;
         var type =  cache.GetOrAdd(typename, x => 
            Type.GetType(x, false, ignorecCase) 
            ?? AppDomain.CurrentDomain.GetExportedTypes().FirstOrDefault(t => string.Equals(t.FullName, x, comparer)));
         if (type == null && throwOnError)
         {
            throw new TypeLoadException("Type not found: " + typename);
         }
         return type;
      }

   }
}
