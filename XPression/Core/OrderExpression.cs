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
using System.Linq.Expressions;
using System.Text;

namespace XPression.Core
{
   public class OrderExpression
   {
      public static List<Tuple<string, bool>> Tokenize(string expression)
      {
         if (expression == null) throw new ArgumentNullException("expression");
         var result = new List<Tuple<string, bool>>();
         var column = new StringBuilder();
         var order = new StringBuilder();
         var readingOrder = false;
         var descending = false;
         foreach (var ch in expression)
         {
            switch (ch)
            {
               case '-':
                  if (column.Length == 0)
                  {
                     descending = true;
                     continue;
                  }
                  break;
               case ',':
                  if (order.Length > 0)
                  {
                     var s = order.ToString().ToLower();
                     descending = s == "d" || s == "desc";
                  }
                  result.Add(Tuple.Create(column.ToString(), descending));
                  column.Length = 0;
                  order.Length = 0;
                  readingOrder = false;
                  descending = false;
                  continue;
               case ' ':
               case '\t':
               case '\n':
               case '\r':
                  if (column.Length > 0)
                  {
                     readingOrder = true;
                  }
                  continue;
            }
            if (readingOrder)
            {
               order.Append(ch);
            }
            else
            {
               column.Append(ch);
            }
         }
         if (column.Length > 0)
         {
            if (order.Length > 0)
            {
               var s = order.ToString().ToLower();
               descending = s == "d" || s == "desc";
            }
            result.Add(Tuple.Create(column.ToString(), descending));
         }
         return result;
      }

      public static IEnumerable<Tuple<Expression,bool>> Parse(Type entityType, string expression, char splitChar='.', bool? ignoreCase=null)
      {
         if (entityType == null) throw new ArgumentNullException("entityType");
         if (expression == null) throw new ArgumentNullException("expression");
         var p = Expression.Parameter(entityType, "p");
         return Tokenize(expression)
            .Select(t => Tuple.Create((Expression)Expression.Lambda(p.GetMemberExpression(t.Item1, splitChar, ignoreCase),p), t.Item2));
      }

      public static IEnumerable<Tuple<Expression, bool>> Parse<TEntity>(string expression, char splitChar = '.',
         bool? ignoreCase = null)
      {
         return Parse(typeof (TEntity), expression, splitChar, ignoreCase);
      }
   }
}
