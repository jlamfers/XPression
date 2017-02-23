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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using XPression.Core;
using XPression.Core.Tokens;
using XPression.Language.Syntax;

namespace XPression.Language
{
   public class Lexer : ILexer
   {
      private readonly Grammar _grammar;
      private readonly ISyntax _syntax;

      public Lexer(Grammar grammar)
      {
         if (grammar == null) throw new ArgumentNullException("grammar");
         _grammar = grammar;
         _syntax = grammar.Syntax;
      }


      public virtual IEnumerable<Token> Tokenize(string source)
      {

         var tokens = new TokenList();

         var syntaxchars = _grammar.Syntax.SyntaxChars;
         var symbols = _grammar.Syntax.Symbols;
         var constants = _grammar.Syntax.Constants;
         var quote = _grammar.Syntax.Quote;
         var escapedSyntax = false;
         int position = 0;

         var tokenizer = new Tokenizer(source, _grammar.Syntax.NonBreakingIdentifierChars, _grammar.Syntax.Quote, _grammar.Syntax.LineComment);

         try
         {

            while (!tokenizer.Eof())
            {
               tokenizer.SkipSpaces(_syntax.Spaces);

               if (tokenizer.Eof())
               {
                  return tokens;
               }

               position = tokenizer.Position + 1;
               var ch = tokenizer.Peek();

               Token token;
               TokenType tokenType;
               if (!escapedSyntax && syntaxchars.TryGetValue(ch, out tokenType))
               {
                  #region Handle syntax chars first

                  switch (tokenType)
                  {
                     case TokenType.SyntaxEscape:
                        tokenizer.Next();
                        escapedSyntax = true;
                        continue;

                     case TokenType.Negate:
                        TokenType? tokenType1;
                        token = new Token
                        {
                           Type = tokenType, // negate
                           Lexeme = ReadSyntax(tokenizer, out tokenType1),
                           Position = position,
                           Source = source
                        };

                        if (tokenType1 != null)
                        {
                           // more chars have been read, the token read has another meaning...
                           token.Type = tokenType1.Value;
                        }
                        else
                        {
                           var ptoken = tokens.LastOrDefault();
                           if (ptoken != null && !ptoken.IsDelimiter() && !_grammar.IsOperator(ptoken) &&
                               !ptoken.IsLParen() && !ptoken.IsLBracket())
                           {
                              // if any previous token, and ptoken is not: ',' '(' or operator, than it is NOT a negation

                              // does it have another meaning, e.g., substraction?
                              if (symbols.TryGetValue(token.Lexeme, out tokenType))
                              {
                                 // yes
                                 token.Type = tokenType;
                              }
                              else
                              {
                                 // there was no additional meaning defined, so throw error
                                 throw new XPressionException(source, "invalid negate: " + (char) ch, position);
                              }
                           }
                        }
                        break;

                     default:
                        TokenType? t;
                        var lex = ReadSyntax(tokenizer, out t);
                        token = Token.Create(t.GetValueOrDefault(tokenType), source, lex , position);
                        if (token.Type == TokenType.Insignificant)
                        {
                           continue;
                        }
                        if (token.Type == TokenType.UnKnown)
                        {
                           // explicitly marked as unknwon character, probably in incomple or wrong context
                           throw new XPressionException(source, "UnKnown character: " + (char)ch, position);
                        }
                        break;

                  }

                  tokens.Add(token);
                  continue;

                  #endregion
               }


               string lexeme;
               Tokenizer.StringTokenType stringTokenType;

               if (escapedSyntax)
               {
                  // try read a numeric or identifier token (not a date, time, duration, etc...)
                  if (!tokenizer.TryReadNumeric(out lexeme, out stringTokenType))
                  {
                     if (!tokenizer.TryReadIdentifier(out lexeme, out stringTokenType))
                     {
                        // could this happen?
                        // ignore escape
                        escapedSyntax = false;
                        lexeme = tokenizer.NextToken(out stringTokenType);
                     }
                  }
               }
               else
               {
                  // let the tokenizer decide what will be the next token
                  lexeme = tokenizer.NextToken(out stringTokenType);
               }

               switch (stringTokenType)
               {
                  case Tokenizer.StringTokenType.Comment:
                     continue;

                  case Tokenizer.StringTokenType.Identifier:

                     if (tokenizer.PeekChar() == quote)
                     {
                        // it is a type specifier, followed by a string value
                        // ignore escape
                        TypeParser parser;
                        if (!_grammar.TryGetTypeParser(lexeme, out parser))
                        {
                           throw new XPressionException(source, "invalid type: " + lexeme, position);
                        }
                        position = tokenizer.Position;
                        lexeme = tokenizer.NextToken(out stringTokenType);
                        token = LiteralToken.CreateLiteral(source, lexeme, position, parser.Parse(lexeme));
                        break;
                     }

                     if (!escapedSyntax)
                     {
                        // find out if the identifier is a syntax related symbol
                        object value;
                        if (constants.TryGetValue(lexeme, out value))
                        {
                           // it is a constant, like: true,false,null,....
                           token = LiteralToken.CreateLiteral(source, lexeme, position, value);
                           break;
                        }

                        if (symbols.TryGetValue(lexeme, out tokenType))
                        {
                           // it may be an operator, like: like,mod,contains,....
                           token = Token.Create(tokenType, source, lexeme, position);
                        }
                        else
                        {
                           // default: it is an identifier
                           token = Token.Create(TokenType.Identifier, source, lexeme, position);
                        }
                     }
                     else
                     {
                        // escaped: always an identifier
                        token = Token.Create(TokenType.Identifier, source, lexeme, position);
                     }

                     break;

                  case Tokenizer.StringTokenType.String:
                     token = LiteralToken.CreateLiteral(source, lexeme, position, lexeme);
                     break;

                  case Tokenizer.StringTokenType.Date:
                  case Tokenizer.StringTokenType.DateTime:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,
                        XmlConvert.ToDateTime(lexeme, XmlDateTimeSerializationMode.Local));
                     break;

                  case Tokenizer.StringTokenType.DateUtc:
                  case Tokenizer.StringTokenType.DateTimeUtc:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,
                        XmlConvert.ToDateTime(lexeme, XmlDateTimeSerializationMode.Utc));
                     break;

                  case Tokenizer.StringTokenType.DateOffset:
                  case Tokenizer.StringTokenType.DateTimeOffset:
                     token = LiteralToken.CreateLiteral(source, lexeme, position, XmlConvert.ToDateTimeOffset(lexeme));
                     break;

                  case Tokenizer.StringTokenType.Time:
                     token = LiteralToken.CreateLiteral(source, lexeme, position, XmlConvert.ToDateTime("2000-01-01T" + lexeme,XmlDateTimeSerializationMode.Local).TimeOfDay);
                     break;
                  case Tokenizer.StringTokenType.TimeUtc:
                  case Tokenizer.StringTokenType.TimeOffset:
                     token = LiteralToken.CreateLiteral(source, lexeme, position, XmlConvert.ToDateTimeOffset("2000-01-01T"+lexeme).ToUniversalTime().TimeOfDay);
                     break;

                  case Tokenizer.StringTokenType.Duration:
                     token = LiteralToken.CreateLiteral(source, lexeme, position, XmlConvert.ToTimeSpan(lexeme));
                     break;

                  case Tokenizer.StringTokenType.Int16:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,
                        Int16.Parse(lexeme, CultureInfo.InvariantCulture));
                     break;
                  case Tokenizer.StringTokenType.Int32:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,
                        Int32.Parse(lexeme, CultureInfo.InvariantCulture));
                     break;
                  case Tokenizer.StringTokenType.Int64:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,
                        Int64.Parse(lexeme, CultureInfo.InvariantCulture));
                     break;
                  case Tokenizer.StringTokenType.Single:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,
                        Single.Parse(lexeme, CultureInfo.InvariantCulture));
                     break;
                  case Tokenizer.StringTokenType.Double:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,
                        Double.Parse(lexeme, CultureInfo.InvariantCulture));
                     break;
                  case Tokenizer.StringTokenType.Decimal:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,
                        Decimal.Parse(lexeme, CultureInfo.InvariantCulture));
                     break;
                  case Tokenizer.StringTokenType.Guid:
                     token = LiteralToken.CreateLiteral(source, lexeme, position,Guid.Parse(lexeme));
                     break;
                  case Tokenizer.StringTokenType.JSON:
                     token = LiteralToken.CreateLiteral(source, lexeme, position, lexeme); //or else parse JObject??
                     break;

                  case Tokenizer.StringTokenType.Hexadecimal:
                     object hexvalue;
                     if (lexeme.Length <= 4) hexvalue = Convert.ToByte(lexeme, 16);
                     else if (lexeme.Length <= 6) hexvalue = Convert.ToUInt16(lexeme, 16);
                     else if (lexeme.Length <= 10) hexvalue = Convert.ToUInt32(lexeme, 16);
                     else hexvalue = Convert.ToUInt64(lexeme, 16);
                     token = LiteralToken.CreateLiteral(source, lexeme, position, hexvalue);
                     break;

                     //case Tokenizer.StringTokenType.Char:
                  default:
                     throw new XPressionException(source, "invalid token: " + lexeme, position);
               }

               escapedSyntax = false;
               tokens.Add(token);

            }
            return tokens;
         }
         catch (Exception ex)
         {
            throw new XPressionException(source,ex.Message,position,ex);
         }
      }

      private string ReadSyntax(Tokenizer tokenizer, out TokenType? symbolType)
      {
         symbolType = null;
         var sb = new StringBuilder().Append(tokenizer.NextChar());
         for (var i = 0; i < 2; i++)
         {
            var ch = tokenizer.Peek();
            if (!_syntax.IsSyntaxChar(ch) || _syntax.IsParen(ch))
            {
               break;
            }
            sb.Append(tokenizer.NextChar());
         }
         var probe = sb.ToString();
         if (probe.Length == 1)
         {
            // one char read, let the lexer decide about the token type
            return probe;
         }
         while (true)
         {
            TokenType st;
            if (_grammar.Syntax.Symbols.TryGetValue(probe, out st))
            {
               symbolType = st;
               return probe;
            }
            if (probe.Length == 1)
            {
               return probe;
            }
            probe = probe.Substring(0, probe.Length - 1);
            tokenizer.Position--;
         }
      }

   }
}
