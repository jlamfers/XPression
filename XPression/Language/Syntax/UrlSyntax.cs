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
using System.Xml;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Core.Tokens;
using XPression.Language.FunctionBuilders;

namespace XPression.Language.Syntax
{
   // Syntx to be used in URL segments
   // basically it looks like ODATA
   // differences are:
   // identifier delimiter is '.' 
   // '~' is added to spaces set
   // added binary string operators ct (contains), sw (startswith), ew (endswith)
   public class UrlSyntax<TExtender> : BaseSyntax<TExtender> where TExtender : new()
   {

      public static readonly UrlSyntax<TExtender> Instance = new UrlSyntax<TExtender>();

      protected UrlSyntax()
         : base(true)
      {

         Spaces = new HashSet<char>(new[]{' ','\r','\n','\t','~'});

         SyntaxChars.Add(new Dictionary<int, TokenType>
         {
            {'-',TokenType.Negate },
            {'(',TokenType.LParen },
            {')',TokenType.RParen },
            {',',TokenType.Delimiter },
            {'.',TokenType.IdentifierDelimiter }
         });

         Symbols.Add(new Dictionary<string, TokenType>
         {
            {"and",TokenType.LogicalAnd },
            {"or",TokenType.LogicalOr },

            {"not",TokenType.LogicalNot },

            {"mod",TokenType.Mod },
            {"div",TokenType.Div },
            {"add",TokenType.Add },
            {"mul",TokenType.Mul },
            {"sub",TokenType.Sub },

            {"has",TokenType.Has },

            {"eq",TokenType.Equal },
            {"ne",TokenType.NotEqual },
            {"lt",TokenType.LessThan },
            {"gt",TokenType.GreaterThan },
            {"le",TokenType.LessThanOrEqual },
            {"ge",TokenType.GreaterThanOrEqual },

            {"ct",TokenType.Contains },
            {"sw",TokenType.StartsWith },
            {"ew",TokenType.EndsWith }

         });

         Functions.Add( new List<FunctionMap>
        {
           new FunctionMap("cast",new CastBuilder()),
           new FunctionMap("isof",new IsofBuilder()),

           new FunctionMap("contains",MemberTokens.String.Contains),
           new FunctionMap("startswith",MemberTokens.String.StartsWith),
           new FunctionMap("endswith",MemberTokens.String.EndsWith),
           new FunctionMap("concat",MemberTokens.String.Concat),
           new FunctionMap("indexof",MemberTokens.String.IndexOf),
           new FunctionMap("length",MemberTokens.String.Length),
           new FunctionMap("substring",MemberTokens.String.Substring1),
           new FunctionMap("substring",MemberTokens.String.Substring2), // overload
           new FunctionMap("toupper",MemberTokens.String.ToUpper),
           new FunctionMap("tolower",MemberTokens.String.ToLower),
           new FunctionMap("trim",MemberTokens.String.Trim),

           new FunctionMap("floor",MemberTokens.Math.FloorDecimal),
           new FunctionMap("floor",MemberTokens.Math.FloorDouble),
           new FunctionMap("round",MemberTokens.Math.RoundDecimalZeroDigits),
           new FunctionMap("round",MemberTokens.Math.RoundDoubleZeroDigits),
           new FunctionMap("ceiling",MemberTokens.Math.CeilingDecimal),
           new FunctionMap("ceiling",MemberTokens.Math.CeilingDouble),

           new FunctionMap("year",MemberTokens.DateTime.Year),
           new FunctionMap("year",MemberTokens.DateTimeOffset.Year),

           new FunctionMap("month",MemberTokens.DateTime.Month),
           new FunctionMap("month",MemberTokens.DateTimeOffset.Month),

           new FunctionMap("day",MemberTokens.DateTime.Day),
           new FunctionMap("day",MemberTokens.DateTimeOffset.Day),
           new FunctionMap("day",MemberTokens.TimeSpan.Days),

           new FunctionMap("hour",MemberTokens.DateTime.Hour),
           new FunctionMap("hour",MemberTokens.DateTimeOffset.Hour),
           new FunctionMap("hour",MemberTokens.TimeSpan.Hours),

           new FunctionMap("minute",MemberTokens.DateTime.Minute),
           new FunctionMap("minute",MemberTokens.DateTimeOffset.Minute),
           new FunctionMap("minute",MemberTokens.TimeSpan.Minutes),

           new FunctionMap("second",MemberTokens.DateTime.Second),
           new FunctionMap("second",MemberTokens.DateTimeOffset.Second),
           new FunctionMap("second",MemberTokens.TimeSpan.Seconds),

           new FunctionMap("fractionalseconds",MemberTokens.DateTime.Millisecond),
           new FunctionMap("fractionalseconds",MemberTokens.DateTimeOffset.Millisecond),
           new FunctionMap("fractionalseconds",MemberTokens.TimeSpan.Milliseconds),

           new FunctionMap("totalfractionalseconds",MemberTokens.TimeSpan.TotalMilliseconds),
           new FunctionMap("totalseconds",MemberTokens.TimeSpan.TotalSeconds),
           new FunctionMap("totalminutes",MemberTokens.TimeSpan.TotalMinutes),
           new FunctionMap("totalhours",MemberTokens.TimeSpan.TotalHours),
           new FunctionMap("totaldays",MemberTokens.TimeSpan.TotalDays),
           new FunctionMap("totaloffsetminutes",new TotalOffsetMinutesBuilder()),

           new FunctionMap("date",MemberTokens.DateTimeOffset.Date),
           new FunctionMap("time",MemberTokens.DateTimeOffset.TimeOfDay),
           new FunctionMap("time",MemberTokens.DateTime.TimeOfDay),
           new FunctionMap("now",MemberTokens.DateTimeOffset.Now),
           new FunctionMap("utcnow",MemberTokens.DateTimeOffset.UtcNow), // not OData specified

           new FunctionMap("mindatetime",MemberTokens.DateTimeOffset.MinValue),
           new FunctionMap("maxdatetime",MemberTokens.DateTimeOffset.MaxValue),


           new FunctionMap("dow",MemberTokens.DateTimeOffset.DayOfWeek), // not OData specified
           new FunctionMap("dow",MemberTokens.DateTime.DayOfWeek)
        });

         this.TryAddSqlServerSpatialTypes();


         Constants.Add(new Dictionary<string, object>
         {
            {"null",null },
            {"true",true},
            {"false",false},
            {"NaN",double.NaN},
            {"INF",double.PositiveInfinity}
         });

         KnownTypes.Add(new Dictionary<string, TypeParser>
         {
            {"X", new TypeParser(typeof (byte[]), lex => lex.ParseBinary())},
            {"binary", new TypeParser(typeof (byte[]), lex => lex.ParseBinary())},
            {"string", new TypeParser(typeof (string), lex => lex)},
            {"boolean", new TypeParser(typeof (bool), lex => bool.Parse(lex))},
            {"byte", new TypeParser(typeof (byte), lex => byte.Parse(lex))},
            {"datetime", new TypeParser(typeof (DateTime), lex => XmlConvert.ToDateTime(lex, XmlDateTimeSerializationMode.Local))},
            {"decimal", new TypeParser(typeof (decimal), lex => decimal.Parse(lex))},
            {"double", new TypeParser(typeof (double), lex => double.Parse(lex))},
            {"single", new TypeParser(typeof (float), lex => float.Parse(lex))},
            {"float", new TypeParser(typeof (float), lex => float.Parse(lex))},
            {"guid", new TypeParser(typeof (Guid), lex => Guid.Parse(lex))},
            {"int16", new TypeParser(typeof (Int16), lex => Int16.Parse(lex))},
            {"int32", new TypeParser(typeof (Int32), lex => int.Parse(lex))},
            {"int64", new TypeParser(typeof (Int64), lex => long.Parse(lex))},
            {"sbyte", new TypeParser(typeof (sbyte), lex => sbyte.Parse(lex))},
            {"time", new TypeParser(typeof (TimeSpan), lex => XmlConvert.ToTimeSpan(lex))},
            {"duration", new TypeParser(typeof (TimeSpan), lex => XmlConvert.ToTimeSpan(lex))},
            {"datetimeoffset", new TypeParser(typeof (DateTimeOffset), lex => XmlConvert.ToDateTimeOffset(lex))}

         });

         CompleteInitialization();

      }

   }
}
