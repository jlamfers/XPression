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
using System.Reflection;

// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable EqualExpressionComparison

namespace XPression.Core
{
   public static class MemberTokens
   {

      public static class Object
      {
         public new static readonly MethodInfo
         ToString = MemberHelper.GetMethodInfo<object>(s => s.ToString());

         public static readonly MethodInfo
            ObjectEquals = MemberHelper.GetMethodInfo(() => Equals(null,null));
      }

      public static class String
      {
         public static readonly MethodInfo
            StartsWith = MemberHelper.GetMethodInfo<string>(s => s.StartsWith("")),
            EndsWith = MemberHelper.GetMethodInfo<string>(s => s.EndsWith("")),
            Contains = MemberHelper.GetMethodInfo<string>(s => s.Contains("")),
            Compare = MemberHelper.GetMethodInfo(() => string.Compare("", "")),
            CompareTo = MemberHelper.GetMethodInfo<string>(s => s.CompareTo("")),
            EqualsStatic = MemberHelper.GetMethodInfo(() => string.Equals("", "")),
            ToUpper = MemberHelper.GetMethodInfo<string>(s => s.ToUpper()),
            ToLower = MemberHelper.GetMethodInfo<string>(s => s.ToLower()),
            Concat = MemberHelper.GetMethodInfo(() => string.Concat("", "")),
            IndexOf = MemberHelper.GetMethodInfo<string>(s => s.IndexOf("")),
            Substring1 = MemberHelper.GetMethodInfo<string>(s => s.Substring(0)),
            Substring2 = MemberHelper.GetMethodInfo<string>(s => s.Substring(0,0)),
            Trim = MemberHelper.GetMethodInfo<string>(s => s.Trim()),
            FormatWithProvider = MemberHelper.GetMethodInfo(() => string.Format(null,"",new object[0]))
            ;

         public new static readonly MethodInfo
            Equals = MemberHelper.GetMethodInfo<string>(s => s.Equals(""));


         public static readonly PropertyInfo
             Length = MemberHelper.GetPropertyInfo<string>(s => s.Length);

      }

      public static class Enum
      {
         public static readonly MethodInfo
            HasFlag = MemberHelper.GetMethodInfo<System.Enum>(x => x.HasFlag(null));
      }

      public static class DateTime
      {
         public static readonly MethodInfo
            AddMilliseconds = MemberHelper.GetMethodInfo<System.DateTime>(x => x.AddMilliseconds(0)),
            AddSeconds = MemberHelper.GetMethodInfo<System.DateTime>(x => x.AddSeconds(0)),
            AddMinutes = MemberHelper.GetMethodInfo<System.DateTime>(x => x.AddMinutes(0)),
            AddHours = MemberHelper.GetMethodInfo<System.DateTime>(x => x.AddHours(0)),
            AddDays = MemberHelper.GetMethodInfo<System.DateTime>(x => x.AddDays(0)),
            AddMonths = MemberHelper.GetMethodInfo<System.DateTime>(x => x.AddMonths(0)),
            AddYears = MemberHelper.GetMethodInfo<System.DateTime>(x => x.AddYears(0)),
            AddDuration = MemberHelper.GetMethodInfo<System.DateTime>(x => x.Add(System.TimeSpan.MinValue));

         public static readonly PropertyInfo
            TimeOfDay = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.TimeOfDay),
            Now = MemberHelper.GetPropertyInfo(() => System.DateTime.Now),
            UtcNow = MemberHelper.GetPropertyInfo(() => System.DateTime.UtcNow),
            Today = MemberHelper.GetPropertyInfo(() => System.DateTime.Today),
            Date = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.Date),
            Year = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.Year),
            Month = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.Month),
            Day = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.Day),
            DayOfWeek = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.DayOfWeek),
            DayOfYear = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.DayOfYear),
            Hour = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.Hour),
            Minute = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.Minute),
            Second = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.Second),
            Millisecond = MemberHelper.GetPropertyInfo<System.DateTime>(d => d.Millisecond);

         public static readonly FieldInfo
             MinValue = MemberHelper.GetFieldInfo(() => System.DateTime.MinValue),
             MaxValue = MemberHelper.GetFieldInfo(() => System.DateTime.MaxValue);
      }

      public static class DateTimeOffset
      {
         public static readonly MethodInfo
             AddMilliseconds = MemberHelper.GetMethodInfo<System.DateTimeOffset>(x => x.AddMilliseconds(0)),
             AddSeconds = MemberHelper.GetMethodInfo<System.DateTimeOffset>(x => x.AddSeconds(0)),
             AddMinutes = MemberHelper.GetMethodInfo<System.DateTimeOffset>(x => x.AddMinutes(0)),
             AddHours = MemberHelper.GetMethodInfo<System.DateTimeOffset>(x => x.AddHours(0)),
             AddDays = MemberHelper.GetMethodInfo<System.DateTimeOffset>(x => x.AddDays(0)),
             AddMonths = MemberHelper.GetMethodInfo<System.DateTimeOffset>(x => x.AddMonths(0)),
             AddYears = MemberHelper.GetMethodInfo<System.DateTimeOffset>(x => x.AddYears(0)),
             AddDuration = MemberHelper.GetMethodInfo<System.DateTimeOffset>(x => x.Add(System.TimeSpan.MinValue));

         public static readonly PropertyInfo
            Offset = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Offset),
            TimeOfDay = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.TimeOfDay),
            Now = MemberHelper.GetPropertyInfo(() => System.DateTimeOffset.Now),
            UtcNow = MemberHelper.GetPropertyInfo(() => System.DateTimeOffset.UtcNow),
            Date = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Date),
            Year = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Year),
            Month = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Month),
            Day = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Day),
            DayOfWeek = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.DayOfWeek),
            DayOfYear = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.DayOfYear),
            Hour = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Hour),
            Minute = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Minute),
            Second = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Second),
            Millisecond = MemberHelper.GetPropertyInfo<System.DateTimeOffset>(d => d.Millisecond);

         public static readonly FieldInfo
             MinValue = MemberHelper.GetFieldInfo(() => System.DateTimeOffset.MinValue),
             MaxValue = MemberHelper.GetFieldInfo(() => System.DateTimeOffset.MaxValue);
      }

      public static class TimeSpan
      {
         public static readonly PropertyInfo
            TotalDays = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.TotalDays),
            TotalHours = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.TotalHours),
            TotalMinutes = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.TotalMinutes),
            TotalSeconds = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.TotalSeconds),
            TotalMilliseconds = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.TotalMilliseconds),
            Days = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.Days),
            Hours = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.Hours),
            Minutes = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.Minutes),
            Seconds = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.Seconds),
            Milliseconds = MemberHelper.GetPropertyInfo<System.TimeSpan>(d => d.Milliseconds);
      }

      public static class Math
      {
         public static readonly MethodInfo
             RoundDouble = MemberHelper.GetMethodInfo(() => System.Math.Round((double)0, 0)),
             RoundDecimal = MemberHelper.GetMethodInfo(() => System.Math.Round((Decimal)0, 0)),
             RoundDoubleZeroDigits = MemberHelper.GetMethodInfo(() => System.Math.Round((double)0)),
             RoundDecimalZeroDigits = MemberHelper.GetMethodInfo(() => System.Math.Round((Decimal)0)),
             FloorDouble = MemberHelper.GetMethodInfo(() => System.Math.Floor((double)0)),
             FloorDecimal = MemberHelper.GetMethodInfo(() => System.Math.Floor((Decimal)0)),
             CeilingDouble = MemberHelper.GetMethodInfo(() => System.Math.Ceiling((double)0)),
             CeilingDecimal = MemberHelper.GetMethodInfo(() => System.Math.Ceiling((Decimal)0)),
             Pow = MemberHelper.GetMethodInfo(() => System.Math.Pow(0,0))
             ;
      }

      public static class Nullable
      {
         public static readonly MethodInfo
            GetBoolValueOrDefault = MemberHelper.GetMethodInfo<bool?>(s => s.GetValueOrDefault());

      }

      public static class Variables
      {
         public static readonly MethodInfo
            Declare = MemberHelper.GetMethodInfo(() => VariablesSpace.Declare(null, null)),
            Set = MemberHelper.GetMethodInfo(() => VariablesSpace.Set(null, null, null)),
            Get = MemberHelper.GetMethodInfo(() => VariablesSpace.Get(null, null));
      }
      public static class VariablesIgnoreCase
      {
         public static readonly MethodInfo
            Set = MemberHelper.GetMethodInfo(() => VariablesSpaceIgnoreCase.Set(null, null, null)),
            Get = MemberHelper.GetMethodInfo(() => VariablesSpaceIgnoreCase.Get(null, null));
      }

      public static class Assignment
      {
         public static readonly MethodInfo
            Set = MemberHelper.GetMethodInfo(() => MemberAssignment.Set(null, null, null));
      }

      public static bool EqualMethods(this MethodInfo self, MethodInfo other)
      {
         if (self == other) return true;
         if (self.Name != other.Name && self.GetParameters().Length != other.GetParameters().Length || (self.IsGenericMethod != other.IsGenericMethod)) return false;
         if (!self.IsGenericMethod) return false;
         return self.GetGenericMethodDefinition() == other.GetGenericMethodDefinition();
      }

   }
}