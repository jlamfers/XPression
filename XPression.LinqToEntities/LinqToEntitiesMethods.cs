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
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Reflection;
using XPression.Core;

namespace XPression.LinqToEntities
{
   public static class LinqToEntitiesMethods
   {

      public static readonly MethodInfo
         CreateTime = MemberHelper.GetMethodInfo(() => DbFunctions.CreateTime(null, null, null)),
         CreateDateTime = MemberHelper.GetMethodInfo(() => DbFunctions.CreateDateTime(null, null, null, null, null, null));

      public static class ForGeography
      {
         public static readonly MethodInfo
            Distance = MemberHelper.GetMethodInfo<DbGeography>(x => x.Distance(DbGeography.FromText(""))),
            Intersects = MemberHelper.GetMethodInfo<DbGeography>(x => x.Intersects(DbGeography.FromText("")));

         public static readonly PropertyInfo
            Length = MemberHelper.GetPropertyInfo<DbGeography>(x => x.Length);
      }

      public static class ForGeometry
      {
         public static readonly MethodInfo
            Distance = MemberHelper.GetMethodInfo<DbGeometry>(x => x.Distance(DbGeometry.FromText(""))),
            Intersects = MemberHelper.GetMethodInfo<DbGeometry>(x => x.Intersects(DbGeometry.FromText("")));

         public static readonly PropertyInfo
            Length = MemberHelper.GetPropertyInfo<DbGeometry>(x => x.Length);

      }


      public static class ForDateTime
      {
         public static readonly MethodInfo
            AddDays = MemberHelper.GetMethodInfo(() => DbFunctions.AddDays(DateTime.MinValue, null)),
            AddMinutes = MemberHelper.GetMethodInfo(() => DbFunctions.AddMinutes(DateTime.MinValue, null)),
            AddMilliseconds = MemberHelper.GetMethodInfo(() => DbFunctions.AddMilliseconds(DateTime.MinValue, null));
      }
      public static class ForDateTimeOffset
      {
         public static readonly MethodInfo
            AddDays = MemberHelper.GetMethodInfo(() => DbFunctions.AddDays(DateTimeOffset.MinValue, null)),
            AddMinutes = MemberHelper.GetMethodInfo(() => DbFunctions.AddMinutes(DateTimeOffset.MinValue, null)),
            AddMilliseconds = MemberHelper.GetMethodInfo(() => DbFunctions.AddMilliseconds(DateTimeOffset.MinValue, null));
      }
   }
}