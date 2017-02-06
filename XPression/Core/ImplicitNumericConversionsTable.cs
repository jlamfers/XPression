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

namespace XPression.Core
{
   // analogous to C# standards
   // see Implicit Numeric Conversions Table (C# Reference)
   // https://msdn.microsoft.com/nl-nl/library/y5b434w4.aspx

   public static class ImplicitNumericConversionsTable
   {
      
      private static readonly IDictionary<Type, HashSet<Type>>
         ConversionTable = new Dictionary<Type, HashSet<Type>>
         {
            {
               typeof(sbyte),
               new HashSet<Type>
               {
                  typeof(short),
                  typeof(int),
                  typeof(long),
                  typeof(float),
                  typeof(double),
                  typeof(decimal)
               }
            },
            {
               typeof(byte),
               new HashSet<Type>
               {
                  typeof(short),
                  typeof(ushort),
                  typeof(int),
                  typeof(uint),
                  typeof(long),
                  typeof(ulong),
                  typeof(float),
                  typeof(double),
                  typeof(decimal)
               }
            },
            {
               typeof(short),
               new HashSet<Type>
               {
                  typeof(int),
                  typeof(long),
                  typeof(float),
                  typeof(double),
                  typeof(decimal)
               }
            },
            {
               typeof(ushort),
               new HashSet<Type>
               {
                  typeof(int),
                  typeof(uint),
                  typeof(long),
                  typeof(ulong),
                  typeof(float),
                  typeof(double),
                  typeof(decimal)
               }
            },
            {
               typeof(int),
               new HashSet<Type>
               {
                  typeof(long),
                  typeof(float),
                  typeof(double),
                  typeof(decimal)
               }
            },
            {
               typeof(uint),
               new HashSet<Type>
               {
                  typeof(long),
                  typeof(ulong),
                  typeof(float),
                  typeof(double),
                  typeof(decimal)
               }
            },
            {
               typeof(long),
               new HashSet<Type>
               {
                  typeof(float),
                  typeof(double),
                  typeof(decimal)
               }
            },
            {
               typeof(ulong),
               new HashSet<Type>
               {
                  typeof(float),
                  typeof(double),
                  typeof(decimal)
               }
            },
            {
               typeof(char),
               new HashSet<Type>
               {
                  typeof(ushort),
                  typeof(int),
                  typeof(uint),
                  typeof(long),
                  typeof(ulong),
                  typeof(float),
                  typeof(double),
                  typeof(decimal)

               }
            },
            {
               typeof(float),
               new HashSet<Type>
               {
                  typeof(double)
               }
            },
         };

      public static Type GetImplicitConversionType(this Type left, Type right)
      {
         return left.GetImplicitConversionType(right, @default:left);
      }
      public static Type GetImplicitConversionType(this Type left, Type right, Type @default)
      {
         HashSet<Type> table;
         var t1 = left.EnsureNotNullable();
         var t2 = right.EnsureNotNullable();

         if (ConversionTable.TryGetValue(t1, out table) && table.Contains(t2))
         {
            return right;
         }
         if (ConversionTable.TryGetValue(t2, out table) && table.Contains(t1))
         {
            return left;
         }
         return @default;
      }
      public static bool CanBeConvertedInto(this Type self, Type other)
      {
         HashSet<Type> set;
         if (other.IsAssignableFrom(self)) return true;
         return ConversionTable.TryGetValue(self.EnsureNotNullable(), out set) && set.Contains(other.EnsureNotNullable());
      }
   }
}
