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
using System.Linq.Expressions;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Core.Tokens;

namespace XPression.Language.FunctionBuilders
{
   public class IffBuilder : IFunctionBuilder
   {
      // C# equivalents:
      // if(test,body) => return test ? body() : true;
      // if(test,trueBody,falseBody) => return test ? trueBody() : falseBody();
      // if parametercount==2 (falseBody is omitted), then the falseBody always 
      // returns true (so then the trueBody MUST return a boolean value as well)
      public bool TryBuildExpression(ASTBuilder treeBuilder, FunctionToken functionToken, Stack<Token> stack, ParameterExpression p, out Expression output)
      {
         var ifFalse = functionToken.ParameterCount >= 3 ? treeBuilder.PopExpression(stack, p) : Expression.Constant(true);
         var ifTrue = treeBuilder.PopExpression(stack, p);
         var test = treeBuilder.PopExpression(stack, p);
         output = Expression.Condition(test.Convert<bool>(), ifTrue, ifFalse);
         return true;
      }
   }
}