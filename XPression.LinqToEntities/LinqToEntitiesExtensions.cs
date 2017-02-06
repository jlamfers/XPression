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
using XPression.Core;

namespace XPression.LinqToEntities
{
   public static class LinqToEntitiesExtensions
   {
      private static readonly Parser DefaultParser = new ODataParser();

      public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, string predicate, Parser parser = null, bool linqToEntities = true)
      {
         parser = parser ?? DefaultParser;
         var lambda = parser.Parse<TSource, bool>(predicate);

         // simplify is needed to evaluate all independent subtrees. These not always can be handled by EF
         lambda = lambda.Simplify();

         if (linqToEntities)
         {
            lambda = lambda.AsLinqToEntities();
         }

         return source.Where(lambda);
      }

      public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, string predicate, Parser parser = null)
      {
         parser = parser ?? DefaultParser;
         var lambda = parser.Parse<TSource, bool>(predicate);
         return source.Where(lambda.Compile());
      }

      public static Expression AsLinqToEntities(this Expression self)
      {
         return new LinqToEntitiesNormalizer().Visit(self);
      }

      public static Expression<Func<TEntity, bool>> AsLinqToEntities<TEntity>(this Expression<Func<TEntity, bool>> self)
      {
         return (Expression<Func<TEntity, bool>>)new LinqToEntitiesNormalizer().Visit(self);
      }


   }
}