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
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace XPression.Core
{
   [Serializable]
   public class XPressionException : Exception
   {

      public XPressionException()
      {
      }

      public XPressionException(string message) : base(message)
      {
      }

      public XPressionException(string message, Exception inner) : base(message, inner)
      {
      }

      public XPressionException(string expression, string message, int position)
         : this(expression, message, position, null)
      {
      }

      public XPressionException(string expression, string message, int position, Exception ex)
         : base(message, ex)
      {
         var other = ex as XPressionException;
         if (other != null && other.Position > 0)
         {
            // keep the most inner possition
            position = other.Position;
         }
         
         Position = position;
         if (expression != null)
         {
            Expression = expression;
            int row;
            int col;
            int lineCount;
            Line = FindErrorLine(expression, position, out row, out col, out lineCount).Replace("\t", " ");
            Row = row;
            Col = col;
            LineCount = lineCount;
         }
      }


      protected XPressionException(
         SerializationInfo info,
         StreamingContext context) : base(info, context)
      {
         Expression = info.GetString("Expression");
         Position = info.GetInt32("Position");
         Row = info.GetInt32("Row");
         Col = info.GetInt32("Col");
         LineCount = info.GetInt32("LineCount");
         Line = info.GetString("Line");
      }

      // The following method serializes the instance.
      [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
      public override void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info,context);
         info.AddValue("Expression", Expression);
         info.AddValue("Position", Position);
         info.AddValue("Row", Row);
         info.AddValue("Col", Col);
         info.AddValue("Line", Line);
         info.AddValue("LineCount", LineCount);
      }

      public string Expression { get; private set; }
      public int Position { get; private set; }
      public int Row { get; private set; }
      public int Col { get; private set; }
      public string Line { get; private set; }
      public int LineCount { get; private set; }

      public string FullMessage
      {
         get
         {
            var msg = base.Message;
            var sb = new StringBuilder();
            if (Line != null)
            {
               var title = 
                  Position > 0
                  ? string.Format("Error in {0} at: Ln {1} Col {2}", LineCount > 1 ? "script" : "expression",Row, Col)
                  : string.Format("Error in {0}:", LineCount > 1 ? "script" : "expression");
               
               sb.AppendLine(title);
               sb.AppendLine();
               sb.AppendLine(Line);
               if (Position > 0)
               {
                  sb.AppendLine("^".PadLeft(Col));
               }
               sb.AppendLine(msg);
               return sb.ToString();
            }
            return base.Message;
         }
      }

      private static string FindErrorLine(string source, int position, out int row, out int col, out int lineCount)
      {
         row = 1;
         col = 0;
         for (var i = 0; i < position; i++)
         {
            var ch = source[i];
            if (ch == '\n')
            {
               row++;
               col = 0;
            }
            else if (ch == '\r')
            {
               col = 0;
            }
            else
            {
               col++;
            }
         }
         var lines = source.Split('\n').Select(s => s.Trim('\r')).ToArray();
         var line = lines[row - 1];
         lineCount = lines.Length;
         if (lineCount > 1 && position == 0)
         {
            line += "...";
         }
         return line;
      }
   }
}
