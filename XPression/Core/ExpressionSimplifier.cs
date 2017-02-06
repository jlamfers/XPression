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

namespace XPression.Core
{
   // thanks to:
   // http://stackoverflow.com/questions/30308124/force-a-net-expression-to-use-current-value

   /// <summary>
   /// Before "sending" a predicate to EF you may need to simplify it first.
   /// Simplify evaluates all independent subtrees which are not dependent 
   /// of the parameter, i.e., can be converted into values, without effecting
   /// the expression's outcome. 
   /// </summary>
   public static class ExpressionSimplifier
   {

      #region Types
      /// <summary>
      /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
      /// </summary>
      private class SubtreeEvaluator : ExpressionVisitor
      {
         private readonly HashSet<Expression> _candidates;

         internal SubtreeEvaluator(HashSet<Expression> candidates)
         {
            _candidates = candidates;
         }

         internal Expression Eval(Expression exp)
         {
            return Visit(exp);
         }

         public override Expression Visit(Expression exp)
         {
            if (exp == null)
            {
               return null;
            }
            if (_candidates.Contains(exp))
            {
               return Evaluate(exp);
            }
            return base.Visit(exp);
         }

         private static Expression Evaluate(Expression e)
         {
            if (e.NodeType == ExpressionType.Constant)
            {
               return e;
            }
            var lambda = Expression.Lambda(e);
            var fn = lambda.Compile();
            return Expression.Constant(fn.DynamicInvoke(null), e.Type);
         }
      }

      /// <summary>
      /// Performs bottom-up analysis to determine which nodes can possibly
      /// be part of an evaluated sub-tree.
      /// </summary>
      private class Nominator : ExpressionVisitor
      {
         private readonly Func<Expression, bool> _canBeEvaluated;
         private HashSet<Expression> _candidates;
         private bool _cannotBeEvaluated;

         internal Nominator(Func<Expression, bool> canBeEvaluated)
         {
            _canBeEvaluated = canBeEvaluated;
         }

         internal HashSet<Expression> Nominate(Expression expression)
         {
            _candidates = new HashSet<Expression>();
            Visit(expression);
            return _candidates;
         }

         public override Expression Visit(Expression expression)
         {
            if (expression != null)
            {
               var saveCannotBeEvaluated = _cannotBeEvaluated;
               _cannotBeEvaluated = false;
               base.Visit(expression);
               if (!_cannotBeEvaluated)
               {
                  if (_canBeEvaluated(expression))
                  {
                     _candidates.Add(expression);
                  }
                  else
                  {
                     _cannotBeEvaluated = true;
                  }
               }
               _cannotBeEvaluated |= saveCannotBeEvaluated;
            }
            return expression;
         }
      }

      #endregion

      /// <summary>
      /// Performs evaluation & replacement of independent sub-trees
      /// </summary>
      /// <param name="expression">The root of the expression tree.</param>
      /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
      public static Expression Simplify(this Expression expression)
      {
         return PartialEval(expression, CanBeEvaluatedLocally);
      }

      /// <summary>
      /// Performs evaluation & replacement of independent sub-trees
      /// </summary>
      /// <param name="expression">The root of the expression tree.</param>
      /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
      public static Expression<Func<T, TResult>> Simplify<T, TResult>(this Expression<Func<T, TResult>> expression)
      {
         return (Expression<Func<T, TResult>>)PartialEval(expression, CanBeEvaluatedLocally);
      }


      /// <summary>
      /// Performs evaluation & replacement of independent sub-trees
      /// </summary>
      /// <param name="expression">The root of the expression tree.</param>
      /// <param name="canBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
      /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
      private static Expression PartialEval(Expression expression, Func<Expression, bool> canBeEvaluated)
      {
         return new SubtreeEvaluator(new Nominator(canBeEvaluated).Nominate(expression)).Eval(expression);
      }


      private static bool CanBeEvaluatedLocally(Expression expression)
      {
         return expression.NodeType != ExpressionType.Parameter;
      }

   }
}
