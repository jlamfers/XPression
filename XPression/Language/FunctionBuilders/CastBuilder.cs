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
using XPression.Core;
using XPression.Core.Functions;
using XPression.Core.Tokens;

namespace XPression.Language.FunctionBuilders
{
   public class CastBuilder : IFunctionBuilder
   {
      public bool TryBuildExpression(ASTBuilder treeBuilder, FunctionToken functionToken, Stack<Token> stack, ParameterExpression p, out Expression output)
      {

         var rightToken = stack.Pop(); // must be a constant string
         var target = treeBuilder.PopExpression(stack, p);
         Type type;
         try
         {
            type = treeBuilder.Grammar.GetType(rightToken.Lexeme);
         }
         catch (Exception ex)
         {
            throw new XPressionException(functionToken.Source,"Invalid type: "+rightToken.Lexeme,rightToken.Position,ex);
         }
         if ( (target.IsNullConstant() || !target.Type.IsValueType || target.Type.IsNullable()) && type.IsValueType && !type.IsNullable())
         {
            type = typeof(Nullable<>).MakeGenericType(type);
         }
         output = target.Convert(type);
         return true;
      }
   }
}