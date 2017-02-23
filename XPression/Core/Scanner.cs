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

namespace XPression.Core
{
   public class Scanner
   {
      private static readonly HashSet<char> DEFAULT_SPACES = new HashSet<char>(new[]{' ','\r','\n','\t'}); 
      public const char NULLCHAR = '\0';
      public const int EOF = -1;


      private readonly string _source;
      private int _position;

      public Scanner(string source)
      {
         if (source == null) throw new ArgumentNullException("source");
         _source = source;
      }

      public void SkipSpaces(ICollection<char> spaces)
      {
         spaces = spaces ?? DEFAULT_SPACES;
         while (spaces.Contains(PeekChar()))
         {
            _position++;
         }
      }

      public string Source
      {
         get { return _source; }
      }
      public int Position
      {
         get { return _position; }
         set { _position = value; }
      }

      public void Seek(int position)
      {
         _position = position;
      }

      public int Next()
      {
         return Eof() ? EOF : _source[_position++];
      }

      public char NextChar()
      {
         return _position < _source.Length ? _source[_position++] : NULLCHAR;
      }

      public string Read(int count)
      {
         var sb = new StringBuilder();
         while (!Eof() && count-- > 0)
         {
            sb.Append(NextChar());
         }
         return sb.ToString();
      }

      public int Peek(int forward)
      {
         return Eof(forward) ? EOF : _source[_position + forward];
      }
      public int Peek()
      {
         return _position < _source.Length ? _source[_position] : EOF;
      }

      public char PeekChar()
      {
         return _position < _source.Length ? _source[_position] : NULLCHAR;
      }
      public char PeekChar(int forward)
      {
         return Eof(forward) ? NULLCHAR : _source[_position + forward];
      }

      public bool PeekAnyOf(params char[] chars)
      {
         var peek = PeekChar();
         foreach (var ch in chars)
         {
            if (ch == peek) return true;
         }
         return false;
      }

      public bool Eof(int forward)
      {
         return (_position + forward) >= _source.Length;
      }
      public bool Eof()
      {
         return _position >= _source.Length;
      }

      public bool PeekIsDigit()
      {
         return char.IsDigit(PeekChar());
      }

   }
}
