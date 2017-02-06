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

using System.Data.Entity.Spatial;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Language.Syntax;

namespace XPression.LinqToEntities
{
   public class SyntaxExtender : IAutoSyntaxExtender
   {
      // Note: overloads are recognized and resolved by XPression
      public void ExtendSyntax(ISyntax syntax)
      {
         //Geography
         syntax.Functions.Add(new FunctionMap("geo.distance", LinqToEntitiesMethods.ForGeography.Distance));
         syntax.Functions.Add(new FunctionMap("geo.intersects", LinqToEntitiesMethods.ForGeography.Intersects));
         syntax.Functions.Add(new FunctionMap("geo.length", LinqToEntitiesMethods.ForGeography.Length));

         //Geometry
         syntax.Functions.Add(new FunctionMap("geo.distance", LinqToEntitiesMethods.ForGeometry.Distance));
         syntax.Functions.Add(new FunctionMap("geo.intersects", LinqToEntitiesMethods.ForGeometry.Intersects));
         syntax.Functions.Add(new FunctionMap("geo.length", LinqToEntitiesMethods.ForGeometry.Length));

         //Types
         syntax.KnownTypes.Add("geography", new TypeParser(typeof(DbGeography), DbGeography.FromText));
         syntax.KnownTypes.Add("geometry", new TypeParser(typeof(DbGeometry), DbGeometry.FromText));
      }
   }
}
