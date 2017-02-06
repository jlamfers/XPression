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
using XPression.Core;

namespace XPression.LinqToEntities
{
   /// <summary>
   /// Substitutes members that cannot be handled by linq-to-entities
   /// (substitutes these by by DbFunctions methods)
   /// </summary>
   internal class LinqToEntitiesNormalizer : ExpressionVisitor
   {
      protected override Expression VisitMember(MemberExpression node)
      {
         if (node.Member == MemberTokens.DateTimeOffset.Date)
         {
            var date = node.Expression.Trim().Convert<DateTimeOffset>();

            return Expression.Call(LinqToEntitiesMethods.CreateDateTime,
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Year).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Month).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Day).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Hour).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Minute).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Second).Convert<double?>()
               )
               .Convert(node.Type);
         }

         if (node.Member == MemberTokens.DateTime.TimeOfDay)
         {
            var date = node.Expression.Trim().Convert<DateTime>();

            return Expression.Call(LinqToEntitiesMethods.CreateTime,
               Expression.MakeMemberAccess(date, MemberTokens.DateTime.Hour).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTime.Minute).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTime.Second).Convert<double?>()
               )
               .Convert(node.Type);
         }

         if (node.Member == MemberTokens.DateTimeOffset.TimeOfDay)
         {
            var date = node.Expression.Trim().Convert<DateTimeOffset>();

            return Expression.Call(LinqToEntitiesMethods.CreateTime,
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Hour).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Minute).Convert<int?>(),
               Expression.MakeMemberAccess(date, MemberTokens.DateTimeOffset.Second).Convert<double?>()
               )
               .Convert(node.Type);
         }
         return base.VisitMember(node);
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
         if (node.Method == MemberTokens.DateTime.AddDuration || node.Method == MemberTokens.DateTimeOffset.AddDuration)
         {
            var left = node.Object.Trim();
            var right = node.Arguments[0].Trim();
            int days;
            int minutes;
            int milliseconds;

            var ts = right.GetValue<TimeSpan?>();

            if (ts == null)
            {
               return Expression.Constant(null).Convert(node.Type);
            }

            GetValues(ts.Value, out days, out minutes, out milliseconds);

            #region DateTime

            if (node.Method == MemberTokens.DateTime.AddDuration)
            {
               left = left.Convert<DateTime?>();

               if (days != 0)
               {
                  left = Expression.Call(LinqToEntitiesMethods.ForDateTime.AddDays, left, days.AsConstant<int?>());
               }
               if (minutes != 0)
               {
                  left = Expression.Call(LinqToEntitiesMethods.ForDateTime.AddMinutes, left, minutes.AsConstant<int?>());
               }
               if (milliseconds != 0)
               {
                  left = Expression.Call(LinqToEntitiesMethods.ForDateTime.AddMilliseconds, left, milliseconds.AsConstant<int?>());
               }
               return left.Convert(node.Type);
            }

            #endregion

            #region DateTimeOffset

            left = left.Convert<DateTimeOffset?>();

            if (days != 0)
            {
               left = Expression.Call(LinqToEntitiesMethods.ForDateTimeOffset.AddDays, left, days.AsConstant<int?>());
            }
            if (minutes != 0)
            {
               left = Expression.Call(LinqToEntitiesMethods.ForDateTimeOffset.AddMinutes, left, minutes.AsConstant<int?>());
            }
            if (milliseconds != 0)
            {
               left = Expression.Call(LinqToEntitiesMethods.ForDateTimeOffset.AddMilliseconds, left, milliseconds.AsConstant<int?>());
            }
            return left.Convert(node.Type);

            #endregion
         }
         return base.VisitMethodCall(node);
      }

      private static void GetValues(TimeSpan ts, out int days, out int minutes, out int milliseconds)
      {
         days = (int)Math.Floor(ts.TotalDays);
         ts = ts.Add(-TimeSpan.FromDays(days));
         minutes = (int)Math.Floor(ts.TotalMinutes);
         ts = ts.Add(-TimeSpan.FromMinutes(minutes));
         milliseconds = (int)ts.TotalMilliseconds;
      }

   }

}