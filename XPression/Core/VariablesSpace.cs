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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace XPression.Core
{
   public class StringComparerOrdinalIgnoreCase : IEqualityComparer<string>
   {
      private IEqualityComparer<string> _comparer = StringComparer.OrdinalIgnoreCase;
      public bool Equals(string x, string y)
      {
         return _comparer.Equals(x, y);
      }

      public int GetHashCode(string obj)
      {
         return _comparer.GetHashCode(obj);
      }
   }
   public class StringComparerOrdinal : IEqualityComparer<string>
   {
      private IEqualityComparer<string> _comparer = StringComparer.Ordinal;
      public bool Equals(string x, string y)
      {
         return _comparer.Equals(x, y);
      }

      public int GetHashCode(string obj)
      {
         return _comparer.GetHashCode(obj);
      }
   }

   public class VariablesSpace<TComparer>
      where TComparer : IEqualityComparer<string>, new()
   {


      private class Bucket
      {
         public object Value;
         public Type Type;
      }

      private class VarDictionary : Dictionary<string, Bucket>
      {
         public VarDictionary()
            : base(new TComparer())
         {
            
         }
      }

      private static readonly ConditionalWeakTable<object, VarDictionary>
         VarTable = new ConditionalWeakTable<object, VarDictionary>();

      public static bool Declare(object target, string name)
      {
         return true;
      }

      public static bool Set(object target, string name, object value)
      {
         var table = VarTable.GetOrCreateValue(target);
         Bucket bucket;
         if (table.TryGetValue(name, out bucket))
         {
            try
            {
               bucket.Value = Convert.ChangeType(value, bucket.Type);
            }
            catch (Exception ex)
            {
               throw new XPressionException(string.Format("value '{0}' cannot be converted into {1}", value, bucket.Type.Name), ex);
            }
         }
         else
         {
            table[name] = new Bucket
            {
               Value = value,
               Type = value == null ? typeof (object) : value.GetType()
            };
         }
         return true;
      }

      public static object Get(object target, string name)
      {
         try
         {
            return VarTable.GetOrCreateValue(target)[name].Value;
         }
         catch(Exception ex)
         {
            throw new XPressionException(string.Format("invalid variable {0}", name),ex);
         }
      }

   }

   public class VariablesSpace : VariablesSpace<StringComparerOrdinal> { }
   public class VariablesSpaceIgnoreCase : VariablesSpace<StringComparerOrdinalIgnoreCase> { }

   public static class MemberAssignment
   {
      public static bool Set(object target, MemberInfo member, object value)
      {
         try
         {
            var pi = member as PropertyInfo;
            if (pi != null)
            {
               pi.SetValue(target, value);
            }
            else
            {
               ((FieldInfo)member).SetValue(target,value);
            }
            return true;
         }
         catch(Exception ex)
         {
            throw new Exception("runtime error - invalid member: " + member,ex);
         }
      }
   }
}
