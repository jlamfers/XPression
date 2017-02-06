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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XPression.Core;
using XPression.Core.Tokens;

namespace XPression.Language
{
   public class TokenList : IEnumerable<Token>
   {
      private class FunctionStackItem
      {
         public FunctionToken Token;
         public int ParenDepth;
      }

      private readonly List<Token> 
         _tokens = new List<Token>();

      private readonly Stack<FunctionStackItem> 
         _functionStack = new Stack<FunctionStackItem>();

      private bool
         _delimitedIdentifier;

      private bool
         _logicalStatementSeperatorAdded;

      public TokenList Add(Token token)
      {

         if (_delimitedIdentifier)
         {
            ((SegmentedIdentifierToken)_tokens.Last()).AddSegment(token.Lexeme);
            _delimitedIdentifier = false;
            return this;
         }

         FunctionStackItem onTopFunction;
         Token lastToken;

         //////////////////////////////////////////////////////////////////////
         // added forgiveness for double statement seperators
         if (_logicalStatementSeperatorAdded)
         {
            if (token.Type == TokenType.LogicalAnd)
            {
               // simply ignore doubles
               return this;
            }
            if (!token.IsIdentifierOrLiteral() && !(token.IsFunctionalUnaryOperator() || token.Type == TokenType.Declaration || token.Type == TokenType.LogicalNot) && !token.IsLParen())
            {
               // remove the last added logicalAnd (which most probably only was meant to be a statement seperator)
               _tokens.RemoveAt(_tokens.Count - 1);
            }
            _logicalStatementSeperatorAdded = false;
         }
         // only if the logicalEnd is defined as a ';' char
         _logicalStatementSeperatorAdded = token.Type == TokenType.LogicalAnd && token.Lexeme == ";";
         //
         //////////////////////////////////////////////////////////////////////

         switch (token.Type)
         {
            case TokenType.IdentifierDelimiter:
               _delimitedIdentifier = true;
               lastToken = _tokens.Last();
               var lastSegmented = lastToken as SegmentedIdentifierToken;
               if (lastSegmented == null)
               {
                  lastSegmented = new SegmentedIdentifierToken(token.Lexeme)
                  {
                     Source = lastToken.Source,
                     Position = lastToken.Position
                  };
                  lastSegmented.AddSegment(lastToken.Lexeme);
                  _tokens.RemoveAt(_tokens.Count - 1);
                  _tokens.Add(lastSegmented);
               }
               return this; // exit

            case TokenType.LParen:
               lastToken = _tokens.LastOrDefault();
               if (lastToken != null && lastToken.Type == TokenType.Identifier)
               {
                  // any identifier before a LParen turns into a function 
                  var t = new FunctionToken { Lexeme = lastToken.Lexeme, Position = lastToken.Position, Source = lastToken.Source };
                  _functionStack.Push(new FunctionStackItem { ParenDepth = 1, Token = t });
                  _tokens[_tokens.Count - 1] = t;
               }
               else
               {
                  if (_functionStack.Any())
                  {
                     _functionStack.First().ParenDepth++;
                  }
               }
               break;

            case TokenType.LArrayBracket:
               lastToken = _tokens.LastOrDefault();
               if (lastToken != null && lastToken.Type == TokenType.Identifier)
               {
                  // any identifier before a LParen turns into a function 
                  var t = new FunctionToken { Lexeme = lastToken.Lexeme, Position = lastToken.Position, Source = lastToken.Source, Type = TokenType.Array };
                  _functionStack.Push(new FunctionStackItem { ParenDepth = 1, Token = t });
                  _tokens[_tokens.Count - 1] = t;
               }
               else
               {
                  throw new XPressionException(token.Source, "invalid char: " + token.Lexeme, token.Position + 1);
               }
               break;

            case TokenType.RParen:
               if (_functionStack.Any())
               {
                  if (--(_functionStack.First().ParenDepth) == 0 && _functionStack.First().Token.IsFunction())
                  {
                     _functionStack.Pop();
                  }
               }
               break;

            case TokenType.RArrayBracket:
               _functionStack.Pop();
               break;

            case TokenType.Delimiter:
               if (!_functionStack.Any())
               {
                  throw new XPressionException(token.Source, "invalid char: " + token.Lexeme, token.Position + 1);
               }
               onTopFunction = _functionStack.First();
               if (onTopFunction.ParenDepth == 1)
               {
                  onTopFunction.Token.ParameterCount += 1;
               }
               break;

            default:
               var top = _functionStack.FirstOrDefault();
               if (top != null && top.Token.ParameterCount == 0)
               {
                  // when we are here, and any function is on the stack,
                  // then the on-top function has at least one argument
                  top.Token.ParameterCount = 1;
               }
               break;
         }

         _tokens.Add(token);

         return this;
      }

      public IEnumerator<Token> GetEnumerator()
      {
         return _tokens.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}