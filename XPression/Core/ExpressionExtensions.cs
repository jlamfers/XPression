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
using System.Linq.Expressions;

namespace XPression.Core
{
   public static class ExpressionExtensions
   {
      public static Expression Trim(this Expression node)
      {
         switch (node.NodeType)
         {
            case ExpressionType.Convert:
            case ExpressionType.Quote:
               return Trim(node.CastTo<UnaryExpression>().Operand);
            default:
               return node;
         }
      }
      public static T GetValue<T>(this Expression node)
      {
         var c = node as ConstantExpression;
         if (c != null)
         {
            return (T)((ConstantExpression)node).Value;
         }
         try
         {
            var lambda = Expression.Lambda(node);
            var fn = lambda.Compile();
            return (T)fn.DynamicInvoke(null);
         }
         catch (Exception ex)
         {
            throw new XPressionException("Unable to resolve timespan. Timespan must be an undependent value.", ex);
         }
      }
      public static Expression Convert<T>(this Expression node)
      {
         return Convert(node, typeof(T));
      }
      public static Expression Convert(this Expression node, Type type)
      {
         return node.Type != type ? Expression.Convert(node.Trim() , type) : node;
      }
      public static Expression AsConstant<T>(this object value)
      {
         return Convert<T>(Expression.Constant(value));
      }
      public static bool IsNullConstant(this Expression self)
      {
         var c = self as ConstantExpression;
         if (c != null && c.Value == null) return true;
         var u = self as UnaryExpression;
         return u != null && IsNullConstant(u.Operand);
      }
   }
}
