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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Core.Tokens;

namespace XPression.Language.FunctionBuilders
{
   public class FormatBuilder : IFunctionBuilder
   {
      public bool TryBuildExpression(ASTBuilder treeBuilder, FunctionToken functionToken, Stack<Token> stack, ParameterExpression p, out Expression output)
      {
         if (functionToken.ParameterCount == 0)
         {
            throw new XPressionException(functionToken.Source,"format requires at least one argument",functionToken.Position);
         }
         var args = new List<Expression>();
         for (var i = 0; i < functionToken.ParameterCount; i++)
         {
            args.Add(treeBuilder.PopExpression(stack, p)); 
         }

         args.Reverse(); // reverse stack order

         output = 
            Expression.Call(
               MemberTokens.String.FormatWithProvider,
               Expression.Constant(CultureInfo.InvariantCulture),
               args[0],
               Expression.NewArrayInit(
                  typeof(object),
                  args.Skip(1).Select(e => e.Type==typeof(object) ? e : Expression.Convert(e,typeof(object)))
               )
            );
         return true;
      }
   }
}