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

namespace XPression.Core.Tokens
{
   /// <summary>
   /// These token types define all types of tokens that can be handled and interpretated
   /// by the Lexer and ASTBuilder
   /// </summary>
   public enum TokenType
   {
      UnKnown,
      Insignificant,

      /// <summary>
      /// An expression token represents a token on the stack that has been compiled yet into an AST (Expression), by the ASTBuilder
      /// </summary>
      Expression, 

      /// <summary>
      /// An identifier represents a name
      /// </summary>
      Identifier,

      /// <summary>
      /// A literal represents a value
      /// </summary>
      Literal,

      /// <summary>
      /// A function represents a function call
      /// </summary>
      Function,

      /// <summary>
      /// An array represents an array access
      /// </summary>
      Array,

      /// <summary>
      /// A delimiter represents a function arguments delimiter
      /// </summary>
      Delimiter,

      /// <summary>
      /// Array indexer
      /// </summary>
      LArrayBracket,
      RArrayBracket,

      /// <summary>
      /// An identifier delimiter represents a relation between properties
      /// like in Person.Address (and Person/Address in OData)
      /// </summary>
      IdentifierDelimiter,

      /// <summary>
      /// A SyntaxEscape indicates that an identifier/literal should not be interpreted as a (part of a) syntax related token.
      /// Example: SyntaxEscape='@'
      /// @null => interpretated as identifier/member "null" instead of value NULL
      /// @2002-02-01 => interpretated as (most likely) ((2002 -2) - 1) => 1999, instead of date => 1st feb 2002
      /// </summary>
      SyntaxEscape,

      LParen,
      RParen,

      LogicalOr,
      LogicalAnd,
      LogicalNot,

      Negate,

      BitwiseOr,
      BitwiseAnd,
      BitwiseXOr,
      LeftShift,
      RightShift,
      BitwiseNot,

      Mod, 
      Div, 
      Mul, 
      Add, 
      Sub,
      Pow,

      /// <summary>
      /// from OData context -> if field Has flag-name, it means that (field BitwiseAnd flag-value NotEqual 0)
      /// </summary>
      Has,

      Equal,
      LessThan,
      GreaterThan,
      LessThanOrEqual,
      GreaterThanOrEqual,
      NotEqual,

      FunctionalUnaryOperator,
      FunctionalBinaryOperator,

      Contains, // can be handled as an operator
      StartsWith,// can be handled as an operator
      EndsWith,// can be handled as an operator
      Like,// can be handled as an operator

      Assignment,
      Declaration
   }
}