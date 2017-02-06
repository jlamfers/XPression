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
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace XPression.Core
{
   public static class ConstantParser
   {
      public static object Parse(Type type, string text)
      {
         try
         {
            if (text == null)
            {
               return null;
            }
            if (type == typeof (string))
            {
               return text;
            }
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof (TimeSpan))
            {
               return XmlConvert.ToTimeSpan(text);
            }
            if (type == typeof (DateTime))
            {
               return XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.Unspecified);
            }
            if (type == typeof (DateTimeOffset))
            {
               return XmlConvert.ToDateTimeOffset(text);
            }


            if (char.IsDigit(text[0]))
            {
               var last = text.Last();
               if (!char.IsDigit(last))
               {
                  switch (last)
                  {
                     case 'm':
                     case 'M':
                     case 'f':
                     case 'F': // no protocol
                     case 'd':
                     case 'D': // no protocol
                     case 'L':
                        text = text.Substring(0, text.Length - 1);
                        break;
                  }
               }
            }
            return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(text);
         }
         catch (Exception ex)
         {
            throw new Exception("Invalid "+type.Name+": " + text,ex);
         }
      }

   }
}
