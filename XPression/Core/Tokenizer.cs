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
using System.Text;
using XPression.Core.Tokens;

namespace XPression.Core
{
   public class Tokenizer : Scanner
   {

      private char _quote;

      public enum StringTokenType
      {
         Comment,
         Eof,
         Char,

         Identifier,

         String,

         Date,
         DateUtc,
         DateOffset,

         DateTime,
         DateTimeUtc,
         DateTimeOffset,

         Time,
         TimeUtc,
         TimeOffset,

         Duration,

         Int16,
         Int32,
         Int64,
         Single,
         Double,
         Decimal,

         Hexadecimal,

         Guid,
         JSON
      }


      private readonly ICollection<char> _nonBreakingIdentifierChars;
      private char _lineComment;

      public Tokenizer(string source, ICollection<char> nonBreakingIdentifierChars, char quote, char lineComment)
         : base(source)
      {
         if (source == null) throw new ArgumentNullException("source");
         _quote = quote;
         _lineComment = lineComment;
         _nonBreakingIdentifierChars = nonBreakingIdentifierChars != null && nonBreakingIdentifierChars.Count > 0 ? nonBreakingIdentifierChars : null;
      }

      public string NextToken(out StringTokenType type)
      {

         if (Eof())
         {
            type = StringTokenType.Eof;
            return null;
         }

         string token;

         if (TryReadComment(out token, out type))
         {
            return token;
         }

         if (TryReadString(out token, out type))
         {
            return token;
         }

         if (TryReadGuid(out token, out type))
         {
            return token;
         }

         if (PeekIsDigit())
         {
            if (TryReadHex(out token, out type))
            {
               return token;
            }

            if (TryReadDate(out token, out type))
            {
               return token;
            }

            if (TryReadTime(out token, out type))
            {
               return token;
            }

            if (TryReadNumeric(out token, out type))
            {
               return token;
            }
         }

         if (TryReadDuration(out token, out type))
         {
            return token;
         }

         if (TryReadJson(out token, out type))
         {
            return token;
         }

         if (TryReadIdentifier(out token, out type))
         {
            return token;
         }
         type = StringTokenType.Char;
         return Read(1);
      }

      public bool TryReadComment(out string token, out StringTokenType type)
      {
         token = null;
         type = StringTokenType.Char;
         if (Peek() != _lineComment)
         {
            return false;
         }
         var sb = new StringBuilder();
         while (!Eof() && PeekChar() != '\n')
         {
            sb.Append(NextChar());
         }
         token = sb.ToString();
         type = StringTokenType.Comment;
         return true;
      }

      //{00000000-0000-0000-0000-000000000000}
      public bool TryReadGuid(out string token, out StringTokenType type)
      {
         type = StringTokenType.Char;
         token = null;
         if (PeekChar() == '{')
         {
            if (PeekChar(9) == '-' && PeekChar(14) == '-' && PeekChar(19) == '-' && PeekChar(24) == '-' && PeekChar(37) == '}')
            {
               var current = Position;
               var guidString = Read(38);
               Guid guid;
               if (!Guid.TryParse(guidString, out guid))
               {
                  Position = current;
                  return false;
               }
               token = guidString;
               type = StringTokenType.Guid;
               return true;
            }
            return false;
         }
         if (PeekChar(8) == '-' && PeekChar(13) == '-' && PeekChar(18) == '-' && PeekChar(23) == '-')
         {
            var current = Position;
            var guidString = Read(36);
            Guid guid;
            if (!Guid.TryParse(guidString, out guid))
            {
               Position = current;
               return false;
            }
            token = guidString;
            type = StringTokenType.Guid;
            return true;
         }
         return false;
      }
      public bool TryReadJson(out string token, out StringTokenType type)
      {
         type = StringTokenType.Char;
         token = null;
         if (PeekChar() == '{')
         {
            var sb = new StringBuilder();
            sb.Append(NextChar());
            var bracketCount = 1;
            while (!Eof())
            {
               switch (PeekChar())
               {
                  case '{':
                     bracketCount++;
                     break;
                  case '}':
                     bracketCount--;
                     if (bracketCount == 0)
                     {
                        sb.Append(NextChar());
                        token = sb.ToString();
                        type = StringTokenType.JSON;
                        return true;
                     }
                     break;
                  case'\"' :
                  case '\'':
                     sb.Append(ReadJsonString(PeekChar()));
                     continue;
               }
               sb.Append(NextChar());
            }
         }
         return false;
      }
      public bool TryReadNumeric(out string token, out StringTokenType type)
      {
         token = null;
         type = StringTokenType.Char;

         if (!PeekIsDigit())
         {
            return false;
         }

         var dotCount = 0;
         var expCount = 0;
         var sb = new StringBuilder();

         while (true)
         {
            var peek = PeekChar();
            if (char.IsDigit(peek))
            {
               sb.Append(NextChar());
               continue;
            }
            switch (peek)
            {
               case 'e':
               case 'E':
                  if (expCount++ > 0)
                  {
                     token = sb.ToString();
                     type = StringTokenType.Double;
                     return true;
                  }
                  sb.Append(NextChar());
                  if (PeekChar() == '-')
                  {
                     sb.Append(NextChar());
                  }
                  continue;

               case '.':
                  if (expCount > 0 || dotCount++ > 0)
                  {
                     token = sb.ToString();
                     type = StringTokenType.Double;
                     return true;
                  }
                  sb.Append(NextChar());
                  continue;

               case 'm':
               case 'M':
                  NextChar();
                  token = sb.ToString();
                  type = StringTokenType.Decimal;
                  return true;

               case 'f':
               case 'F': // no protocol
                  NextChar();
                  token = sb.ToString();
                  type = StringTokenType.Single;
                  return true;

               case 'd':
               case 'D': // no protocol
                  NextChar();
                  token = sb.ToString();
                  type = StringTokenType.Double;
                  return true;

               case 'L':
                  NextChar();
                  token = sb.ToString();
                  type = StringTokenType.Int64;
                  return true;

               default:
                  token = sb.ToString();
                  type = dotCount > 0 || expCount > 0 ? StringTokenType.Double : StringTokenType.Int32;
                  return true;

            }
         }
      }
      public bool TryReadString(out string token, out StringTokenType type)
      {
         type = StringTokenType.Char;
         token = null;
         if (PeekChar() != _quote)
         {
            return false;
         }
         NextChar();
         var sb = new StringBuilder();
         while (!Eof())
         {
            var ch = NextChar();
            if (ch == _quote)
            {
               if (PeekChar() == _quote)
               {
                  // escaped quote
                  sb.Append(NextChar());
               }
               else
               {
                  type = StringTokenType.String;
                  token = sb.ToString();
                  return true;
               }
            }
            else
            {
               sb.Append(ch);
            }
         }
         throw new XPressionException(Source, "unterminated string", Position - sb.Length);
      }
      public bool TryReadHex(out string token, out StringTokenType type)
      {
         type = StringTokenType.Char;
         token = null;
         if (PeekChar() == '0')
         {
            var peek = PeekChar(1);

            // try hex
            if ((peek == 'x' || peek == 'X'))
            {
               var sb = new StringBuilder();
               sb.Append(NextChar());
               sb.Append(NextChar());
               while (PeekChar().IsHex())
               {
                  sb.Append(NextChar());
               }
               type = StringTokenType.Hexadecimal;
               token = sb.ToString();
               return true;
            }
         }
         return false;

      }
      public bool TryReadDate(out string token, out StringTokenType type)
      {
         token = null;
         type = StringTokenType.Char;

         if (PeekChar(4) != '-' || PeekChar(7) != '-')
         {
            return false;
         }

         var position = Position;
         var sb = new StringBuilder();

         for (var i = 0; i < 10; i++)
         {
            if (i != 4 && i != 7 && !PeekIsDigit())
            {
               Seek(position);
               return false;
            }
            sb.Append(NextChar());
         }

         switch (PeekChar())
         {
            case 'Z':
               // UTC Date : 2002-09-24Z
               sb.Append(NextChar());
               type = StringTokenType.DateUtc;
               token = sb.ToString();
               return true;

            case '+':
            case '-':

               position = Position;
               token = sb.ToString();
               type = StringTokenType.Date;

               // Date offset : 2002-09-24+06:00  2002-09-24+06:00
               sb.Append(NextChar());
               for (var i = 0; i < 5; i++)
               {
                  // read offset
                  if (i == 2)
                  {
                     if (PeekChar() != ':')
                     {
                        Seek(position);
                        return true;
                     }
                  }
                  else if (!PeekIsDigit())
                  {
                     Seek(position);
                     return true;
                  }
                  sb.Append(NextChar());
               }
               type = StringTokenType.DateOffset;
               token = sb.ToString();
               return true;

            case 'T':
               // DateTime : 2002-09-24T06:00:00  2002-09-24T06:00:00.100
               // Or DateTimeOffset :
               sb.Append(NextChar());
               string timeToken;
               StringTokenType timeType;
               if (!TryReadTime(out timeToken, out timeType))
               {
                  Seek(position);
                  return false;
               }
               sb.Append(timeToken);
               switch (timeType)
               {
                  case StringTokenType.Time:
                     type = StringTokenType.DateTime;
                     break;
                  case StringTokenType.TimeUtc:
                     type = StringTokenType.DateTimeUtc;
                     break;
                  case StringTokenType.TimeOffset:
                     type = StringTokenType.DateTimeOffset;
                     break;
                  default:
                     Seek(position);
                     return false;
               }
               token = sb.ToString();
               return true;

            default:
               type = StringTokenType.Date;
               token = sb.ToString();
               return true;
         }
      }
      public bool TryReadTime(out string token, out StringTokenType type)
      {
         token = null;
         type = StringTokenType.Char;

         if (PeekChar(2) != ':' || PeekChar(5) != ':')
         {
            return false;
         }

         var position = Position;
         var sb = new StringBuilder();

         for (var i = 0; i < 8; i++)
         {
            if (i != 2 && i != 5 && !PeekIsDigit())
            {
               Seek(position);
               return false;
            }
            sb.Append(NextChar());
         }
         if (PeekChar() == '.')
         {
            // read thousands
            sb.Append(NextChar());
            while (PeekIsDigit())
            {
               sb.Append(NextChar());
            }
         }

         switch (PeekChar())
         {
            case 'Z':
               // UTC Datetime : 2002-09-24T06:00:00.000Z
               sb.Append(NextChar());
               type = StringTokenType.TimeUtc;
               token = sb.ToString();
               return true;

            case '+':
            case '-':
               position = Position;
               type = StringTokenType.Time;
               token = sb.ToString();
               // Timeoffset : 2002-09-24T06:00:00.000+06:00  2002-09-24T06:00:00.000-06:00
               sb.Append(NextChar());
               for (var i = 0; i < 5; i++)
               {
                  // read offset
                  if (i == 2)
                  {
                     if (PeekChar() != ':')
                     {
                        Seek(position);
                        return true;
                     }
                  }
                  else if (!PeekIsDigit())
                  {
                     Seek(position);
                     return true;
                  }
                  sb.Append(NextChar());
               }
               type = StringTokenType.TimeOffset;
               token = sb.ToString();
               return true;

            default:
               type = StringTokenType.Time;
               token = sb.ToString();
               return true;
         }

      }
      public bool TryReadDuration(out string token, out StringTokenType type)
      {
         token = null;
         type = StringTokenType.Char;

         if (PeekChar() != 'P')
         {
            return false;
         }

         var position = Position;
         var sb = new StringBuilder();

         sb.Append(NextChar());
         if (PeekIsDigit())
         {
            while (PeekIsDigit())
            {
               sb.Append(NextChar());
            }
            if (!PeekAnyOf('Y', 'M', 'D'))
            {
               Seek(position);
               return false;
            }
            sb.Append(NextChar());
            if (PeekIsDigit())
            {
               while (PeekIsDigit())
               {
                  sb.Append(NextChar());
               }
               if (!PeekAnyOf('M', 'D'))
               {
                  Seek(position);
                  return false;
               }
               sb.Append(NextChar());
               if (PeekIsDigit())
               {
                  while (PeekIsDigit())
                  {
                     sb.Append(NextChar());
                  }
                  if (PeekChar() != 'D')
                  {
                     Seek(position);
                     return false;
                  }
                  sb.Append(NextChar());
               }
            }
            if (PeekChar() != 'T')
            {
               token = sb.ToString();
               type = StringTokenType.Duration;
               return true;
            }
            sb.Append(NextChar());
         }
         else
         {
            if (PeekChar() != 'T')
            {
               Seek(position);
               return false;
            }
            sb.Append(NextChar());
         }
         // read time part
         while (PeekIsDigit())
         {
            sb.Append(NextChar());
         }
         if (!PeekAnyOf('H', 'M', 'S'))
         {
            Seek(position);
            return false;
         }
         sb.Append(NextChar());
         if (PeekIsDigit())
         {
            while (PeekIsDigit())
            {
               sb.Append(NextChar());
            }
            if (!PeekAnyOf('M', 'S'))
            {
               Seek(position);
               return false;
            }
            sb.Append(NextChar());
            if (PeekIsDigit())
            {
               while (PeekIsDigit())
               {
                  sb.Append(NextChar());
               }
               if (PeekChar() != 'S')
               {
                 Seek(position);
                  return false;
               }
               sb.Append(NextChar());
            }
         }
         token = sb.ToString();
         type = StringTokenType.Duration;
         return true;
      }
      public bool TryReadIdentifier(out string token, out StringTokenType type)
      {
         token = null;
         type = StringTokenType.Char;
         var peek = PeekChar();
         if ( peek != '_' && !char.IsLetter(peek) && (_nonBreakingIdentifierChars == null || !_nonBreakingIdentifierChars.Contains(peek)))
         {
            return false;
         }
         var sb = new StringBuilder();
         sb.Append(NextChar());
         while (!Eof())
         {
            peek = PeekChar();
            if (char.IsLetterOrDigit(peek) || peek == '_')
            {
               sb.Append(NextChar());
            }
            else
            {
               if (_nonBreakingIdentifierChars == null || !_nonBreakingIdentifierChars.Contains(peek))
               {
                  break;
               }
               sb.Append(NextChar());
            }
         }
         type = StringTokenType.Identifier;
         token = sb.ToString();
         return true;
         
      }

      private string ReadJsonString(char quote)
      {
         var sb = new StringBuilder();
         sb.Append(NextChar()); // quote
         var escape = false;
         while (!Eof())
         {
            char ch;
            sb.Append(ch = NextChar());
            if (ch == '\\')
            {
               if (!escape)
               {
                  escape = true;
                  continue;
               }
            }
            else if (ch == quote)
            {
               if (!escape)
               {
                  return sb.ToString();
               }
            }
            escape = false;
         }
         throw new XPressionException(Source, "unterminated JSON string", Position - sb.Length);
      }


   }
}
