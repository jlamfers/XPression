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
using System.Reflection;
using System.Xml;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Core.Tokens;
using XPression.Language.FunctionBuilders;

namespace XPression.Language.Syntax
{
   public class ScriptSyntax<TExtender> : BaseSyntax<TExtender>
      where TExtender : new() {

      public static readonly ScriptSyntax<TExtender> Instance = new ScriptSyntax<TExtender>();

      protected ScriptSyntax() : base(false)
      {
         LineComment = '#';

         NonBreakingIdentifierChars.Add('$');

         SyntaxChars.Add(new Dictionary<int, TokenType>
         {
            {'-', TokenType.Negate},
            {'(', TokenType.LParen},
            {')', TokenType.RParen},
            {'[', TokenType.LArrayBracket},
            {']', TokenType.RArrayBracket},
            {',', TokenType.Delimiter},
            {';', TokenType.LogicalAnd}, // statement seperarator
            {'|', TokenType.BitwiseOr},
            {'&', TokenType.BitwiseAnd},
            {'^', TokenType.BitwiseXOr},
            {'!', TokenType.LogicalNot},
            {'<', TokenType.LessThan},
            {'>', TokenType.GreaterThan},
            {'~', TokenType.BitwiseNot},
            {'/', TokenType.Div},
            {'*', TokenType.Mul},
            {'+', TokenType.Add},
            {'.', TokenType.IdentifierDelimiter},
            {'=', TokenType.Assignment},
            {'@', TokenType.SyntaxEscape}
         });


         Constants.Add(new Dictionary<string, object>
         {
            {"PI", Math.PI},
            {"E", Math.E},
            {"null", null},
            {"true", true},
            {"false", false},
            {"NaN", double.NaN},
            {"INF", double.PositiveInfinity}
         });


         Symbols.Add(new Dictionary<string, TokenType>
         {
            {"-", TokenType.Sub},
            {"==", TokenType.Equal},
            {"!=", TokenType.NotEqual},
            {"||", TokenType.LogicalOr},
            {"&&", TokenType.LogicalAnd},
            {">>", TokenType.RightShift},
            {"<<", TokenType.LeftShift},
            {"var", TokenType.Declaration}
         });


        Functions.Add(new List<FunctionMap>
         {
            new FunctionMap("if", new IffBuilder()),
            new FunctionMap("else", new ElseBuilder()),
            new FunctionMap("format", new FormatBuilder()),
            new FunctionMap("cast", new CastBuilder()),
            new FunctionMap("contains", MemberTokens.String.Contains),
            new FunctionMap("startswith", MemberTokens.String.StartsWith),
            new FunctionMap("endswith", MemberTokens.String.EndsWith),
            new FunctionMap("concat", MemberTokens.String.Concat),
            new FunctionMap("indexof", MemberTokens.String.IndexOf),
            new FunctionMap("length", MemberTokens.String.Length),
            new FunctionMap("substring", MemberTokens.String.Substring1),
            new FunctionMap("substring", MemberTokens.String.Substring2), // overload
            new FunctionMap("toupper", MemberTokens.String.ToUpper),
            new FunctionMap("tolower", MemberTokens.String.ToLower),
            new FunctionMap("trim", MemberTokens.String.Trim),

            new FunctionMap("year", MemberTokens.DateTime.Year),
            new FunctionMap("year", MemberTokens.DateTimeOffset.Year),

            new FunctionMap("month", MemberTokens.DateTime.Month),
            new FunctionMap("month", MemberTokens.DateTimeOffset.Month),

            new FunctionMap("day", MemberTokens.DateTime.Day),
            new FunctionMap("day", MemberTokens.DateTimeOffset.Day),
            new FunctionMap("day", MemberTokens.TimeSpan.Days),

            new FunctionMap("hour", MemberTokens.DateTime.Hour),
            new FunctionMap("hour", MemberTokens.DateTimeOffset.Hour),
            new FunctionMap("hour", MemberTokens.TimeSpan.Hours),

            new FunctionMap("minute", MemberTokens.DateTime.Minute),
            new FunctionMap("minute", MemberTokens.DateTimeOffset.Minute),
            new FunctionMap("minute", MemberTokens.TimeSpan.Minutes),

            new FunctionMap("second", MemberTokens.DateTime.Second),
            new FunctionMap("second", MemberTokens.DateTimeOffset.Second),
            new FunctionMap("second", MemberTokens.TimeSpan.Seconds),

            new FunctionMap("fractionalseconds", MemberTokens.DateTime.Millisecond),
            new FunctionMap("fractionalseconds", MemberTokens.DateTimeOffset.Millisecond),
            new FunctionMap("fractionalseconds", MemberTokens.TimeSpan.Milliseconds),

            new FunctionMap("totalfractionalseconds", MemberTokens.TimeSpan.TotalMilliseconds),
            new FunctionMap("totalseconds", MemberTokens.TimeSpan.TotalSeconds),
            new FunctionMap("totalminutes", MemberTokens.TimeSpan.TotalMinutes),
            new FunctionMap("totalhours", MemberTokens.TimeSpan.TotalHours),
            new FunctionMap("totaldays", MemberTokens.TimeSpan.TotalDays),

            new FunctionMap("date", MemberTokens.DateTimeOffset.Date),
            new FunctionMap("time", MemberTokens.DateTimeOffset.TimeOfDay),
            new FunctionMap("time", MemberTokens.DateTime.TimeOfDay),
            new FunctionMap("now", MemberTokens.DateTimeOffset.Now),
            new FunctionMap("utcnow", MemberTokens.DateTimeOffset.UtcNow),

            new FunctionMap("dow", MemberTokens.DateTimeOffset.DayOfWeek),
            new FunctionMap("dow", MemberTokens.DateTime.DayOfWeek),
         });


         foreach (var m in typeof(TExtender).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => Attribute.IsDefined(m,typeof(ScriptMethodAttribute))))
         {
            Functions.Add(new FunctionMap(m.GetCustomAttribute<ScriptMethodAttribute>().Name, m));
         }

         this.AddMathFunctions();

         KnownTypes.Add(new Dictionary<string, TypeParser>  
         {
            {"X", new TypeParser(typeof (byte[]), lex => lex.ParseBinary())},
            {"string", new TypeParser(typeof (string), lex => lex)},
            {"bool", new TypeParser(typeof (bool), lex => bool.Parse(lex))},
            {"byte", new TypeParser(typeof (byte), lex => byte.Parse(lex))},
            {"dt", new TypeParser(typeof (DateTime), lex => XmlConvert.ToDateTime(lex, XmlDateTimeSerializationMode.Local))},
            {"datetime", new TypeParser(typeof (DateTime), lex => XmlConvert.ToDateTime(lex, XmlDateTimeSerializationMode.Local))},
            {"decimal", new TypeParser(typeof (decimal), lex => decimal.Parse(lex))},
            {"double", new TypeParser(typeof (double), lex => double.Parse(lex))},
            {"float", new TypeParser(typeof (float), lex => float.Parse(lex))},
            {"guid", new TypeParser(typeof (Guid), lex => Guid.Parse(lex))},
            {"int16", new TypeParser(typeof (Int16), lex => Int16.Parse(lex))},
            {"int32", new TypeParser(typeof (Int32), lex => int.Parse(lex))},
            {"int64", new TypeParser(typeof (Int64), lex => long.Parse(lex))},
            {"sbyte", new TypeParser(typeof (sbyte), lex => sbyte.Parse(lex))},
            {"ts", new TypeParser(typeof (TimeSpan), lex => XmlConvert.ToTimeSpan(lex))},
            {"timespan", new TypeParser(typeof (TimeSpan), lex => XmlConvert.ToTimeSpan(lex))},
            {"dto", new TypeParser(typeof (DateTimeOffset), lex => XmlConvert.ToDateTimeOffset(lex))},
            {"datetimeoffset", new TypeParser(typeof (DateTimeOffset), lex => XmlConvert.ToDateTimeOffset(lex))}
         });

         CompleteInitialization();

      }

   }
}
