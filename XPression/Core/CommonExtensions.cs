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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using XPression.Core.Tokens;

namespace XPression.Core
{
   internal static class CommonExtensions
   {
      public static T CastTo<T>(this object self)
      {
         return self == null ? default(T) : (T) self;
      }

      public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> self)
      {
         return self == null ? null : new ReadOnlyDictionary<TKey, TValue>(self);
      }
      public static IDictionary<TKey, TValue> Add<TKey, TValue>(this IDictionary<TKey, TValue> self, IDictionary<TKey, TValue> other)
      {
         foreach (var kv in other)
         {
            self.Add(kv);
         }
         return self;
      }
      public static IDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> self, IDictionary<TKey, TValue> other)
      {
         foreach (var kv in other)
         {
            self[kv.Key] = kv.Value;
         }
         return self;
      }
      public static IList<T> Add<T>(this IList<T> self, IEnumerable<T> other)
      {
         foreach (var kv in other)
         {
            self.Add(kv);
         }
         return self;
      }

      public static ExpressionToken ToToken(this Expression self)
      {
         return new ExpressionToken {Expression = self};
      }

      public static bool IsNullable(this Type self)
      {
         return Nullable.GetUnderlyingType(self) != null;
      }

      public static Type EnsureNotNullable(this Type self)
      {
         return Nullable.GetUnderlyingType(self) ?? self;
      }

      public static Type EnsureNullable(this Type self)
      {
         if (!self.IsValueType || self.IsNullable()) return self;
         return typeof(Nullable<>).MakeGenericType(self);
      }

      // may be used with ODATA grammar
      public static byte[] ParseBinary(this string lexeme)
      {
         if (String.IsNullOrWhiteSpace(lexeme))
         {
            return null;
         }
         if (lexeme.Length % 2 == 0)
         {
            var bytes = new byte[lexeme.Length / 2];

            for (var i = 0; i < bytes.Length; i++)
            {
               if (IsHex(lexeme[i * 2]) && IsHex(lexeme[i * 2 + 1]))
               {
                  bytes[i] = (byte)(HexToInt(lexeme[i * 2]) * 16 + HexToInt(lexeme[i * 2 + 1]));
               }
               else
               {
                  throw new Exception("invalid binary string: " + lexeme);
               }
            }

            return bytes;
         }

         throw new Exception("invalid binary string: " + lexeme);
      }
      public static bool IsHex(this char ch)
      {
         return ((int) ch).IsHex();
      }
      public static bool IsHex(this int ch)
      {
         return ch >= '0' && ch <= '9'
                || ch >= 'a' && ch <= 'f'
                || ch >= 'A' && ch <= 'F';
      }
      public static int HexToInt(this char ch)
      {
         if (ch >= '0' && ch <= '9') return ch - '0';
         if (ch >= 'a' && ch <= 'f') return ch - 'a' + 10;
         if (ch >= 'A' && ch <= 'F') return ch - 'A' + 10;
         throw new ArgumentOutOfRangeException("ch");
      }
   }
}
