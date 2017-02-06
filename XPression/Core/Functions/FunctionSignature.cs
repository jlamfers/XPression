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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XPression.Core.Functions
{
   public class FunctionSignature : IEnumerable<Type>
   {
      private readonly IList<Type> _types;
      private readonly int _hashcode;

      public FunctionSignature(IEnumerable<Type> parameterTypes)
      {
         _types = parameterTypes.ToList().AsReadOnly();
         unchecked
         {
            _hashcode = 1973;
            foreach (var t in _types)
            {
               _hashcode = _hashcode * 2017 + t.GetHashCode();
            }
         }
      }

      public bool IsInvokableBy(FunctionSignature other)
      {
         return other._types.Count == _types.Count && !_types.Where((t, i) => !other._types[i].CanBeConvertedInto(t)).Any();
      }

      public override int GetHashCode()
      {
         return _hashcode;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public IEnumerator<Type> GetEnumerator()
      {
         return _types.GetEnumerator();
      }

      public override bool Equals(object obj)
      {
         var other = obj as FunctionSignature;
         return other != null && _types.SequenceEqual(other._types);
      }

      public IList<Type> ToList()
      {
         return _types;
      } 

   }
}