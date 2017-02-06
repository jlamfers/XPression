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
using System.Linq.Expressions;
using System.Reflection;

namespace XPression.Core
{
   public static class MemberHelper
   {

      public static MethodInfo GetMethodInfo(this Expression<Action> expression)
      {
         return (MethodInfo)GetMemberInfo(expression);
      }
      public static MethodInfo GetMethodInfo<T>(this Expression<Action<T>> expression)
      {
         return (MethodInfo)GetMemberInfo(expression);
      }
      public static MethodInfo GetMethodInfo<T, TResult>(this Expression<Func<T, TResult>> expression)
      {
         return (MethodInfo)GetMemberInfo(expression);
      }

      public static PropertyInfo GetPropertyInfo(this Expression<Func<object>> expression)
      {
         return (PropertyInfo)expression.GetMemberInfo();
      }
      public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> expression)
      {
         return (PropertyInfo)expression.GetMemberInfo();
      }

      public static FieldInfo GetFieldInfo(this Expression<Func<object>> expression)
      {
         return (FieldInfo)expression.GetMemberInfo();
      }
      public static FieldInfo GetFieldInfo<T>(this Expression<Func<T, object>> expression)
      {
         return (FieldInfo)expression.GetMemberInfo();
      }


      public static MemberInfo GetMemberInfo(this Expression expression, bool throwException = true)
      {
         switch (expression.NodeType)
         {
            case ExpressionType.Lambda:
               return GetMemberInfo(((LambdaExpression)expression).Body);
            case ExpressionType.Convert:
            case ExpressionType.Quote:
               return GetMemberInfo(((UnaryExpression)expression).Operand);
            case ExpressionType.MemberAccess:
               return ((MemberExpression)expression).Member;
            case ExpressionType.Call:
               return ((MethodCallExpression)expression).Method;
            default:
               if (throwException)
                  throw new ArgumentException(string.Format("Cannot obtain a MemberExpression from '{0}' ", expression), "expression");
               return null;
         }
      }

      public static MemberExpression GetMemberExpression(this ParameterExpression self, string name, char splitChar, bool? ignoreCase)
      {
         MemberExpression memberExpression = null;
         Expression target = self;
         var currentMemberTargetType = self.Type;
         foreach (var part in name.Split(splitChar))
         {
            var currentMember = currentMemberTargetType.GetPropertyOrFieldMember(part,ignoreCase);
            if (currentMember == null)
            {
               return null;
            }
            memberExpression = currentMember.GetPropertyOrFieldExpression(target);
            target = memberExpression;
            currentMemberTargetType = currentMember.GetMemberType();
         }
         return memberExpression;
      }
      public static MemberExpression GetMemberExpression(this ParameterExpression self, string name, bool? ignoreCase)
      {
         var currentMember = self.Type.GetPropertyOrFieldMember(name,ignoreCase);
         return currentMember == null ? null : currentMember.GetPropertyOrFieldExpression(self);
      }
      public static MemberExpression GetMemberExpression(this ParameterExpression self, IEnumerable<string> segments, bool? ignoreCase)
      {
         MemberExpression memberExpression = null;
         Expression target = self;
         var currentMemberTargetType = self.Type;
         foreach (var part in segments)
         {
            var currentMember = currentMemberTargetType.GetPropertyOrFieldMember(part, ignoreCase);
            if (currentMember == null)
            {
               return null;
            }
            memberExpression = currentMember.GetPropertyOrFieldExpression(target);
            target = memberExpression;
            currentMemberTargetType = currentMember.GetMemberType();
         }
         return memberExpression;
      }

      public static Type GetMemberType(this MemberInfo member)
      {
         switch (member.MemberType)
         {
            case MemberTypes.Property:
               return ((PropertyInfo) member).PropertyType;
            case MemberTypes.Field:
               return ((FieldInfo)member).FieldType;
            case MemberTypes.Method:
               return ((MethodInfo)member).ReturnType;
            default:
               throw new ArgumentOutOfRangeException();
         }
      }
      public static bool IsStatic(this MemberInfo member)
      {
         switch (member.MemberType)
         {
            case MemberTypes.Property:
               return (((PropertyInfo)member).GetGetMethod(true) ?? ((PropertyInfo)member).GetSetMethod(true)).IsStatic;
            case MemberTypes.Field:
               return ((FieldInfo)member).IsStatic;
            case MemberTypes.Method:
               return ((MethodInfo)member).IsStatic;
            default:
               throw new ArgumentOutOfRangeException();
         }
      }
      public static MemberInfo GetPropertyOrFieldMember(this Type type, string name, bool? ignoreCase)
      {
         const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
         if (ignoreCase.HasValue && !ignoreCase.Value)
         {
            return (MemberInfo)type.GetProperty(name, flags) ?? type.GetField(name, flags);
         }
         const BindingFlags flagsIgnoreCase = flags | BindingFlags.IgnoreCase;
         try
         {
            return (MemberInfo)type.GetProperty(name, flagsIgnoreCase) ?? type.GetField(name, flagsIgnoreCase);
         }
         catch (AmbiguousMatchException)
         {
            return (MemberInfo)type.GetProperty(name, flags) ?? type.GetField(name, flags);
         }

      }
      private static MemberExpression GetPropertyOrFieldExpression(this MemberInfo member, Expression target)
      {
         return member.MemberType == MemberTypes.Property ? Expression.Property(target, (PropertyInfo)member) : Expression.Field(target, (FieldInfo)member);
      }

   }
}
