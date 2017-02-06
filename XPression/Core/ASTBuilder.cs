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
using System.Reflection;
using System.Runtime.CompilerServices;
using XPression.Core.Tokens;

namespace XPression.Core
{
   public class ASTBuilder
   {
      private static class NestedVars
      {
         public class IgnoreCaseStringDictionary : Dictionary<string, Type>
         {
            public IgnoreCaseStringDictionary() : base(StringComparer.OrdinalIgnoreCase){}
         }
         public static readonly ConditionalWeakTable<object, Dictionary<string, Type>> Vars = new ConditionalWeakTable<object, Dictionary<string, Type>>();
         public static readonly ConditionalWeakTable<object, IgnoreCaseStringDictionary> VarsIgnoreCase = new ConditionalWeakTable<object, IgnoreCaseStringDictionary>();

         public static Dictionary<string, Type> GetVars(object stack, bool ignoreCase)
         {
            return ignoreCase ? VarsIgnoreCase.GetOrCreateValue(stack) : Vars.GetOrCreateValue(stack);
         }
      }

      public ASTBuilder(IGrammar grammar)
      {
         Grammar = grammar;
      }

      public IGrammar Grammar { get; private set; }

      public Expression PopExpression(Stack<Token> stack, ParameterExpression p)
      {
         Expression left, right;

         if (stack.Count == 0)
         {
            // it probably is an operator error (too less operators)
            throw new XPressionException("operator error");
         }

         var token = stack.Pop();

         switch (token.Type)
         {
            case TokenType.Expression:
               return ((ExpressionToken)token).Expression;

            case TokenType.Identifier:
            case TokenType.Array:

               var segmented = token as SegmentedIdentifierToken;

               var memberExpression =
                  token.Type != TokenType.Array
                     ? (segmented != null
                        ? p.GetMemberExpression(segmented.Segments, Grammar.IgnoreCase)
                        : p.GetMemberExpression(token.Lexeme, Grammar.IgnoreCase)
                        )
                     : p.GetMemberExpression(token.Lexeme.Split(Grammar.IdentifierDelimiter), Grammar.IgnoreCase);

               if (memberExpression == null && Grammar.ImplementsVariables)
               {
                  Type varType;
                  if (NestedVars.GetVars(stack,Grammar.IgnoreCase).TryGetValue(token.Lexeme, out varType) || Grammar.Strict)
                  {
                     // get variable
                     var getter = Grammar.IgnoreCase
                        ? MemberTokens.VariablesIgnoreCase.Get
                        : MemberTokens.Variables.Get;
                     return Expression.Call(getter, p, Expression.Constant(token.Lexeme)).Convert(varType ?? typeof(object));
                  }
               }

               if (Grammar.Strict)// || segmented != null)
               {
                  if (memberExpression == null)
                  {
                     throw new XPressionException(token.Source, "invalid member: " + token.Lexeme, token.Position);
                  }
               }
               if (token.Type == TokenType.Array)
               {
                  return BuildArrayAccessExpression((FunctionToken)token, memberExpression != null
                     ? (Expression) memberExpression
                     : Expression.Constant(token.Lexeme), stack, p);
               }
               return memberExpression != null
                  ? (Expression)memberExpression
                  : Expression.Constant(token.Lexeme); // when strict is off, handle as a string constant

            case TokenType.Literal:
               var literalToken = (LiteralToken) token;
               if (literalToken.ConvertedValue is TimeSpan && !string.IsNullOrEmpty(token.Lexeme) && token.Lexeme[0] == 'P')
               {
                  // just to be sure....
                  var me = p.GetMemberExpression(token.Lexeme,Grammar.IgnoreCase);
                  if (me != null)
                  {
                     return me;
                  }
               }
               return Expression.Constant(literalToken.ConvertedValue);

            case TokenType.Function:
            case TokenType.FunctionalBinaryOperator:
            case TokenType.FunctionalUnaryOperator:

               try
               {
                  Expression expression;
                  if (Grammar.FunctionBuilder.TryBuildExpression(this, (FunctionToken) token, stack, p, out expression))
                  {
                     return expression;
                  }
               }
               catch (XPressionException)
               {
                  throw;
               }
               catch (Exception ex)
               {
                  throw new XPressionException(token.Source,ex.Message,token.Position,ex);
               }

               throw new XPressionException(token.Source,"unknown function: " + token.Lexeme,token.Position);

            case TokenType.Assignment:
               return BuildAssignmentExpression(stack, p);
            case TokenType.Declaration:
               return BuildDeclarationExpression(stack, p);
            case TokenType.LogicalOr:
               return BuildBinaryExpression(stack, Expression.OrElse, p);
            case TokenType.LogicalAnd:
               return BuildBinaryExpression(stack, Expression.AndAlso, p);
            case TokenType.LogicalNot:
               return BuildUnaryExpression(stack, Expression.Not, p);
            case TokenType.Negate:
               return BuildUnaryExpression(stack, Expression.Negate, p);
            case TokenType.Mod:
               return BuildBinaryExpression(stack, Expression.Modulo, p);
            case TokenType.Div:
               return BuildBinaryExpression(stack, Expression.Divide, p);
            case TokenType.Mul:
               return BuildBinaryExpression(stack, Expression.Multiply, p);
            case TokenType.Pow:
               return BuildBinaryExpression(stack, Expression.Power, p);
            case TokenType.Add:
               return BuildBinaryExpression(stack, Expression.Add, p);
            case TokenType.Sub:
               return BuildBinaryExpression(stack, Expression.Subtract, p);
            case TokenType.Has:
               return BuildBinaryHasExpression(stack, p);

            case TokenType.BitwiseOr:
               return BuildBinaryExpression(stack, Expression.Or, p);
            case TokenType.BitwiseAnd:
               return BuildBinaryExpression(stack, Expression.And, p);

            case TokenType.BitwiseXOr:
               return BuildBinaryExpression(stack, Expression.ExclusiveOr, p);

            case TokenType.LeftShift:
               return BuildBinaryExpression(stack, Expression.LeftShift, p);
            case TokenType.RightShift:
               return BuildBinaryExpression(stack, Expression.RightShift, p);
            case TokenType.BitwiseNot:
               return BuildUnaryExpression(stack, Expression.OnesComplement, p);

            case TokenType.Equal:
               return BuildBinaryExpression(stack, Expression.Equal, p);
            case TokenType.LessThan:
               return BuildBinaryCompareExpressionExceptEqual(stack, Expression.LessThan, p, ExpressionType.LessThan);
            case TokenType.GreaterThan:
               return BuildBinaryCompareExpressionExceptEqual(stack, Expression.GreaterThan, p, ExpressionType.GreaterThan);
            case TokenType.LessThanOrEqual:
               return BuildBinaryCompareExpressionExceptEqual(stack, Expression.LessThanOrEqual, p, ExpressionType.LessThanOrEqual);
            case TokenType.GreaterThanOrEqual:
               return BuildBinaryCompareExpressionExceptEqual(stack, Expression.GreaterThanOrEqual, p, ExpressionType.GreaterThanOrEqual);
            case TokenType.NotEqual:
               return BuildBinaryExpression(stack, Expression.NotEqual, p);

            case TokenType.Like:
               return BuildLikeExpression(stack, token, p);

            case TokenType.Contains:
               right = PopExpression(stack,p);
               left = PopExpression(stack, p);
               return Expression.Call(left, MemberTokens.String.Contains, right);

            case TokenType.StartsWith:
               right = PopExpression(stack, p);
               left = PopExpression(stack, p);
               return Expression.Call(left, MemberTokens.String.StartsWith, right);

            case TokenType.EndsWith:
               right = PopExpression(stack, p);
               left = PopExpression(stack, p);
               return Expression.Call(left, MemberTokens.String.EndsWith, right);

            default:
               throw new ArgumentOutOfRangeException();
         }
      }

      private Expression BuildArrayAccessExpression(FunctionToken token, Expression array, Stack<Token> stack, ParameterExpression p)
      {
         var args = new List<Expression>();
         for (var i = 0; i < token.ParameterCount; i++)
         {
            args.Add(PopExpression(stack, p));
         }
         args.Reverse();

         return Expression.ArrayAccess(array, args);
      }

      private Expression BuildLikeExpression(Stack<Token> stack, Token token, ParameterExpression p)
      {
         var right = PopExpression(stack, p);
         var left = PopExpression(stack, p);
         if (!(right is ConstantExpression) || right.Type != typeof(string))
         {
            throw new XPressionException(token.Source, "right operand must be a string constant", token.Position);
         }
         var pattern = (string)((ConstantExpression)right).Value;
         if (pattern.StartsWith("%"))
         {
            return pattern.EndsWith("%") 
               ? Expression.Call(left, MemberTokens.String.Contains, Expression.Constant(pattern.Substring(1, pattern.Length - 2))) 
               : Expression.Call(left, MemberTokens.String.EndsWith, Expression.Constant(pattern.Substring(1)));
         }
         return pattern.EndsWith("%") 
            ? Expression.Call(left, MemberTokens.String.StartsWith, Expression.Constant(pattern.Substring(0, pattern.Length - 1))) 
            : Expression.Call(left, MemberTokens.String.Equals, right);
      }

      private Expression BuildAssignmentExpression(Stack<Token> stack, ParameterExpression p)
      {
         Type varType;
         Dictionary<string, Type> vars;
         MethodInfo setter;

         var right = PopExpression(stack, p);
         var token = stack.Pop();
         if (token.Type != TokenType.Identifier)
         {
            if (token.Type == TokenType.Expression && Grammar.ImplementsVariables)
            {
               var expr = token.CastTo<ExpressionToken>().Expression as MethodCallExpression;
               if (expr != null && expr.Method == MemberTokens.Variables.Declare)
               {
                  // left token is declaration
                  var name = (string) expr.Arguments[1].CastTo<ConstantExpression>().Value;
                  vars = NestedVars.GetVars(stack,Grammar.IgnoreCase);
                  vars.TryGetValue(name, out varType);
                  if (varType == null)
                  {
                     vars[name] = right.Type;
                  }
                  setter = Grammar.IgnoreCase ? MemberTokens.VariablesIgnoreCase.Set : MemberTokens.Variables.Set;
                  return Expression.And(expr,
                     Expression.Call(setter, p.Convert<object>(), Expression.Constant(name),
                        right.Convert<object>()));
               }
            }
            throw new XPressionException(token.Source, "Expected an identifier or declaration", token.Position);
         }

         var segmented = token as SegmentedIdentifierToken;

         var memberExpression = segmented != null
            ? p.GetMemberExpression(segmented.Segments,Grammar.IgnoreCase)
            : p.GetMemberExpression(token.Lexeme,Grammar.IgnoreCase);

         if (memberExpression != null)
         {
            var memberType = memberExpression.Member.GetMemberType();
            if (!right.Type.CanBeConvertedInto(memberType))
            {
               throw new XPressionException(token.Source, string.Format("Invalid cast. Cannot cast from {0} to {1}", right.Type.Name, memberType.Name), token.Position);
            }
            return Expression.Call(MemberTokens.Assignment.Set, memberExpression.Expression.Convert<object>(), Expression.Constant(memberExpression.Member), right.Convert<object>());
         }

         if (!Grammar.ImplementsVariables)
         {
            throw new XPressionException(token.Source, "Member not found: " + token.Lexeme, token.Position);
         }

         vars = NestedVars.GetVars(stack,Grammar.IgnoreCase);

         if (!vars.TryGetValue(token.Lexeme, out varType))
         {
            throw new XPressionException(token.Source, "Variable not declared: " + token.Lexeme, token.Position);
         }
         if (varType == null)
         {
            vars[token.Lexeme] = (varType=right.Type);
         }
         if (!right.Type.CanBeConvertedInto(varType))
         {
            throw new XPressionException(token.Source,string.Format("Invalid cast. Cannot cast from {0} to {1}",right.Type.Name,varType.Name),token.Position);
         }
         setter = Grammar.IgnoreCase ? MemberTokens.VariablesIgnoreCase.Set : MemberTokens.Variables.Set;
         return Expression.Call(setter, p.Convert<object>(), Expression.Constant(token.Lexeme), right.Convert<object>());
      }

      private Expression BuildDeclarationExpression(Stack<Token> stack, ParameterExpression p)
      {
         var token = stack.Pop();
         if (token.Type != TokenType.Identifier)
         {
            throw new XPressionException(token.Source, "Expected an identifier", token.Position);
         }
         var vars = NestedVars.GetVars(stack,Grammar.IgnoreCase);
         if (vars.ContainsKey(token.Lexeme))
         {
            throw new XPressionException(token.Source, "Duplicate variable declaration: " + token.Lexeme, token.Position);
         }
         if (p.Type.GetPropertyOrFieldMember(token.Lexeme,Grammar.IgnoreCase) != null)
         {
            throw new XPressionException(token.Source, "Invalid variable declaration: " + token.Lexeme, token.Position);
         }
         vars.Add(token.Lexeme,null);
         return Expression.Call(MemberTokens.Variables.Declare, p.Convert<object>(), Expression.Constant(token.Lexeme));
      }

      private Expression BuildBinaryExpression(Stack<Token> self, Func<Expression, Expression, BinaryExpression> f, ParameterExpression p)
      {
         var right = PopExpression(self, p);
         var left = PopExpression(self, p);

         if (left.Type==typeof(object) && left.IsNullConstant())
         {
            left = f == Expression.AndAlso || f == Expression.OrElse
               ? Expression.Convert(Expression.Constant(null), typeof (bool?))
               : Expression.Convert(Expression.Constant(null), right.Type.EnsureNullable());
         }

         if (f == Expression.Equal)
         {
            var leftType = left.Type.EnsureNotNullable();
            if (leftType.IsEnum)
            {
               if (right.IsNullConstant())
               {
                  return Expression.Convert(Expression.Constant(null), typeof (bool?));
               }
               var enumValue = Enum.Parse(leftType, (string)((ConstantExpression)right).Value, true);
               right = Expression.Constant(enumValue);
            }
         }
         else if (f == Expression.Add)
         {
            if (right.Type.EnsureNotNullable() == typeof (TimeSpan))
            {
               if (left.Type.EnsureNotNullable() == typeof(DateTimeOffset))
               {
                  if (left.Type.IsNullable())
                  {
                     left = Expression.Convert(left, left.Type.EnsureNotNullable());
                  }
                  if (right.Type.IsNullable())
                  {
                     right = Expression.Convert(right, right.Type.EnsureNotNullable());
                  }

                  return Expression.Call(left, MemberTokens.DateTimeOffset.AddDuration, right);
               }
               if (left.Type.EnsureNotNullable() == typeof (DateTime))
               {
                  if (left.Type.IsNullable())
                  {
                     left = Expression.Convert(left, left.Type.EnsureNotNullable());
                  }
                  if (right.Type.IsNullable())
                  {
                     right = Expression.Convert(right, right.Type.EnsureNotNullable());
                  }

                  return Expression.Call(left, MemberTokens.DateTime.AddDuration, right);
               }
            }
         }

         else if (f == Expression.Subtract)
         {
            if (right.Type.EnsureNotNullable() == typeof (TimeSpan))
            {
               if (left.Type.EnsureNotNullable() == typeof (DateTimeOffset))
               {
                  if (left.Type.IsNullable())
                  {
                     left = Expression.Convert(left, left.Type.EnsureNotNullable());
                  }
                  if (right.Type.IsNullable())
                  {
                     right = Expression.Convert(right, right.Type.EnsureNotNullable());
                  }

                  return Expression.Call(left, MemberTokens.DateTimeOffset.AddDuration, Expression.Negate(right));
               }
               if (left.Type.EnsureNotNullable() == typeof (DateTime))
               {
                  if (left.Type.IsNullable())
                  {
                     left = Expression.Convert(left, left.Type.EnsureNotNullable());
                  }
                  if (right.Type.IsNullable())
                  {
                     right = Expression.Convert(right, right.Type.EnsureNotNullable());
                  }

                  return Expression.Call(left, MemberTokens.DateTime.AddDuration, Expression.Negate(right));
               }
            }
         }


         EnsureImplicitConversion(ref left, ref right);

         if (f == Expression.Add)
         {
            // allow string additions
            if (left.Type == typeof (string))
            {
               return Expression.Call(MemberTokens.String.Concat, left, right);
            }
         }



         return f(left, right);
      }

      private Expression BuildBinaryCompareExpressionExceptEqual(Stack<Token> self, Func<Expression, Expression, BinaryExpression> f, ParameterExpression p, ExpressionType operatorType)
      {
         var right = PopExpression(self,p);
         var left = PopExpression(self, p);

         if (left.Type == typeof(object) && left.IsNullConstant())
         {
            left = Expression.Convert(Expression.Constant(null), right.Type.EnsureNullable());
         }

         EnsureImplicitConversion(ref left, ref right);

         return left.Type != typeof (string)
            ? f(left, right)
            : Expression.MakeBinary(operatorType, Expression.Call(MemberTokens.String.Compare, left, right),Expression.Constant(0));
      }

      private void EnsureImplicitConversion(ref Expression left, ref Expression right)
      {
         if (left.Type != right.Type)
         {
            var type = left.Type;
            if (type == typeof (string))
            {
               right = right.IsNullConstant() ? (Expression)Expression.Convert(Expression.Constant(null),typeof(string)) : Expression.Call(right,MemberTokens.Object.ToString);
            }
            else if (type.IsValueType && !type.IsNullable() && right.IsNullConstant())
            {
               // so that null values can be handled
               type = typeof (Nullable<>).MakeGenericType(type);
               left = Expression.Convert(left, type);
               right = Expression.Convert(right, type);
            }
            else if (type != typeof(string) && right.Type==typeof(string) && right is ConstantExpression)
            {
               right = Expression.Constant(ConstantParser.Parse(type, (string)((ConstantExpression) right).Value));
            }
            else
            {
               var resultType = left.Type.GetImplicitConversionType(right.Type);
               if (left.Type != resultType)
               {
                  left = Expression.Convert(left, resultType);
               }
               if (right.Type != resultType)
               {
                  right = Expression.Convert(right, resultType);
               }
            }
         }
      }

      private Expression BuildBinaryHasExpression(Stack<Token> self, ParameterExpression p)
      {
         var rightToken = self.Pop();
         var left = PopExpression(self, p);
         if (rightToken.IsNull())
         {
            return Expression.Convert(Expression.Constant(null), typeof(bool?));
         }
         var rightLiteral = rightToken as LiteralToken;
         Type enumType;
         object value;
         if (rightLiteral != null && rightLiteral.ConvertedValue != null && rightLiteral.ConvertedValue.GetType().IsEnum)
         {
            value = rightLiteral.ConvertedValue;
            enumType = value.GetType().EnsureNotNullable();
         }
         else
         {
            enumType = left.Type.EnsureNotNullable();
            value = Enum.Parse(enumType, rightToken.Lexeme, true);
         }
         left = left.Convert(enumType.GetEnumUnderlyingType());
         var right = Expression.Constant(value).Convert(enumType.GetEnumUnderlyingType());
         return Expression.NotEqual(Expression.And(left, right), Expression.Constant(0).Convert(enumType.GetEnumUnderlyingType()));
      }

      private Expression BuildUnaryExpression(Stack<Token> self, Func<Expression, UnaryExpression> f, ParameterExpression p)
      {
         var operand = PopExpression(self,p);
         if (f == Expression.Not)
         {
            if (!typeof (bool?).IsAssignableFrom(operand.Type))
            {
               var t = self.FirstOrDefault();
               throw new XPressionException(t != null ? t.Source : null,"not-operator only can be applied on boolean expressions", t != null ? t.Position : 0);
            }
         }
         return f(operand);
      }
   }
}