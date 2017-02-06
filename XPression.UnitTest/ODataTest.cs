using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using XPression.Core;
using XPression.Language;
using XPression.Language.Syntax;

namespace XPression.UnitTest
{
   [TestFixture]
   public class ODataTest
   {
      private readonly Parser _parser = new Parser(new Grammar(new ODataSyntax()){Strict = true});

      [SetUp]
      public void Setup()
      {
         SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);         
      }

      public class Dates
      {
         public DateTimeOffset? Date1 = DateTimeOffset.Now;
         public DateTime? Date2 = DateTime.Now;
         public TimeSpan? Ts = TimeSpan.FromDays(10);
         public TimeSpan? Ts2 = null;
      }

      [Test]
      public void ParenthesWork()
      {
         _p("(1d add 2) div 3 eq 1");
         _p("1d add 2d div 3 eq ((2d div 3) add 1)");
      }

      [Test]
      public void SegmentedPropertiesWork()
      {
         _p("Inner/Name eq 'foo'", new { Inner = new { Name = "foo" } });
      }

      [Test]
      public void DurationCanBeAddedToDate()
      {

         var c = new Dates();
         _p("Date1 add P10D gt now()", c);
         _p("Date2 add P10D gt date(now())", c);
         _p("Date2 add Ts gt date(now())", c);
         _p("Ts2 eq null or Date2 add Ts2 gt date(now())", c);
      }

      [Test]
      public void ConstantsWork()
      {
         Assert.IsTrue(ParsePredicate("true"));
         Assert.IsFalse(ParsePredicate("false"));
         Assert.IsFalse(ParsePredicate("null"));
         Assert.IsNull(Parse("null"));
         Assert.AreEqual(Parse("NaN"), double.NaN);
         Assert.AreEqual(Parse("INF"), double.PositiveInfinity);
         Assert.AreEqual(Parse("-INF"), double.NegativeInfinity);
         Assert.AreEqual(Parse("1"), 1);
         Assert.AreEqual(Parse("3.14"), 3.14);
         Assert.AreEqual(Parse("3.14e10"), 3.14e10);
         Assert.AreEqual(Parse("-1"), -1);
         Assert.AreEqual(Parse("-1.3d"), -1.3d);
         Assert.AreEqual(Parse("-1.3f"), -1.3f);
         Assert.AreEqual(Parse("-1.3M"), -1.3M);
         Assert.AreEqual(Parse("-1.3m"), -1.3m);
         Assert.AreEqual(Parse("100000000L"), 100000000L);
         Assert.AreEqual(Parse("-1.3m mul 1.3m"), -1.3m*1.3m);
         Assert.AreEqual(Parse("-1.3d mul 1.3d"), -1.3d * 1.3d);
         Assert.AreEqual(Parse("-1.3f mul 1.3f"), -1.3f * 1.3f);
         Assert.AreEqual(Parse("-1.3d mul 1.3d add 1.2 div 1.3 add 1.3"), -1.3d * 1.3d +1.2 / 1.3 + 1.3);

      }

      [Test]
      public void BinaryBooleanOperatorsWork()
      {
         Assert.IsTrue(ParsePredicate("true or false"));
         Assert.IsTrue(ParsePredicate("false or true"));
         Assert.IsFalse(ParsePredicate("true and false"));
         Assert.IsFalse(ParsePredicate("false and true"));

         Assert.IsTrue(ParsePredicate("true or null"));
         Assert.IsFalse(ParsePredicate("false or null"));
         Assert.IsTrue(ParsePredicate("null or true"));
         Assert.IsFalse(ParsePredicate("null or false"));
         Assert.IsFalse(ParsePredicate("null or null"));
         Assert.IsNull(Parse<bool?>("null or null"));
         Assert.IsNull(Parse<bool?>("null and null"));
      }

      [Test]
      public void NumericsWork()
      {
         ParsePredicate("1.0 eq 1",true);
         ParsePredicate("1.0M eq 1", true);
         ParsePredicate("1.0f eq 1", true);
         ParsePredicate("1.0d eq 1", true);
         ParsePredicate("1.23e2 eq 123", true);
         ParsePredicate("1.23E2d eq 123", true);
         ParsePredicate("1L eq 1", true);

         ParsePredicate("single'1.0' eq 1", true);
         ParsePredicate("decimal'1.0' eq 1", true);
         ParsePredicate("single'1.0' eq 1", true);
         ParsePredicate("double'1.0' eq 1", true);
         ParsePredicate("double'1.23e2' eq 123", true);
         ParsePredicate("double'1.23E2' eq 123", true);
         ParsePredicate("int64'1' eq 1", true);
         ParsePredicate("int32'1' eq 1", true);
         ParsePredicate("int16'1' eq 1", true);
         ParsePredicate("byte'1' eq 1", true);
         ParsePredicate("sbyte'1' eq 1", true);

         ParsePredicate("cast(1,byte) eq 1", true);
         ParsePredicate("cast(1,sbyte) eq 1", true);
         ParsePredicate("cast(1,int16) eq 1", true);
         ParsePredicate("cast(1,int32) eq 1", true);
         ParsePredicate("cast(1,int64) eq 1", true);
         ParsePredicate("cast(1,single) eq 1", true);
         ParsePredicate("cast(1,double) eq 1", true);
         ParsePredicate("cast(1,decimal) eq 1");
         ParsePredicate("cast(1,byte) eq cast(1,int64)", true);
         ParsePredicate("cast(-1,byte) eq 255", true);
         ParsePredicate("cast(null,byte) eq null", true);
      }

      [Flags]
      public enum Gender
      {
         Male = 1,
         Female = 2
      }
      [Test]
      public void MethodsWork()
      {
         var c = new
         {
            CompanyName = "Alfreds Futterkiste",
            City = "Berlin",
            BirthDate = new DateTime(2000, 12, 1),
            Country="Germany",
            StartTime = new DateTimeOffset(2000, 12, 8,1,0,0,new TimeSpan(0,1,0,0)),
            EndTime = new DateTimeOffset(2000, 12, 9, 1, 0, 0, new TimeSpan(0, 1, 0, 0)),
            StartOfDay = DateTimeOffset.Now.TimeOfDay,
            Freight = 32.457,
            ShipCountry = "Germany",
            Obsolete = (bool?)false,
            CurrentPosition = SqlGeography.Parse(new SqlString("POINT(-122.34900 47.65100)")),
            TargetPosition = SqlGeography.Parse(new SqlString("POINT(-122.34900 47.65100)")),
            DirectRoute = SqlGeography.Parse(new SqlString("POLYGON((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))")),
            TargetArea = SqlGeography.Parse(new SqlString("POLYGON((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))")),
            Position = SqlGeography.Parse(new SqlString("POINT(-122.34900 47.65100)")),
            Gender = Gender.Male
         };

         _p("contains(CompanyName,'freds')", c);
         _p("endswith(CompanyName,'Futterkiste')", c);
         _p("startswith(CompanyName,'Alfr')", c);
         _p("length(CompanyName) eq 19", c);
         _p("indexof(CompanyName,'lfreds') eq 1", c);
         _p("substring(CompanyName,1) eq 'lfreds Futterkiste'", c);
         _p("tolower(CompanyName) eq 'alfreds futterkiste'", c);
         _p("toupper(CompanyName) eq 'ALFREDS FUTTERKISTE'", c);
         _p("trim(CompanyName) eq 'Alfreds Futterkiste'", c);
         _p("concat(concat(City,', '), Country) eq 'Berlin, Germany'", c);
         _p("year(BirthDate) eq 2000", c);
         _p("month(BirthDate) eq 12", c);
         _p("day(StartTime) eq 8",c);
         _p("hour(StartTime) eq 1", c);
         _p("minute(StartTime) eq 0", c);
         _p("second(StartTime) eq 0", c);
         _p("second(StartTime) eq 0", c);
         _p("date(StartTime) ne date(EndTime)", c);
         _p("time(StartTime) le StartOfDay", c);
         _p("totaloffsetminutes(StartTime) eq 60",c);
         _p("StartTime le now()",c);
         _p("StartTime gt mindatetime()",c);
         _p("EndTime lt maxdatetime()",c);
         _p("round(Freight) eq 32",c);
         _p("floor(Freight) eq 32",c);
         _p("ceiling(Freight) eq 33",c);
         _p("cast(ShipCountry,Edm.String) eq ShipCountry", c);
         _p("isof(ShipCountry,Edm.String)",c);
         _p("isof(Obsolete,boolean)", c);
         _p("geo.distance(CurrentPosition,TargetPosition) eq 0.0",c);
         _p("geo.distance(CurrentPosition,TargetPosition) eq 0d", c);
         _p("geo.length(DirectRoute) gt 0.0", c);
         _p("geo.intersects(Position,TargetArea)",c);
         _p("Gender has 'Male'", c);

      }

      private void _p(string expression)
      {
         _p(expression,(object)null);
      }
      private void _p<T>(string expression,T entity = default(T), bool expectedResult=true)
      {
         Expression<Func<T,bool>> e = null;
         try
         {
            Debug.WriteLine(expression);
            e = _parser.Parse<T, bool>(expression);
            Debug.WriteLine("   " + e);
            var result = e.Compile()(entity);
            Assert.AreEqual(expectedResult,result,expression+" expected: "+expectedResult);
         }
         catch (XPressionException ex)
         {
            Debug.WriteLine("expression: " + expression);
            Debug.WriteLine("compiled  : " + e);
            Debug.WriteLine(ex.FullMessage);
            throw;
         }
      }

      private T Parse<T>(string expression)
      {
         Expression<Func<object, T>> e = null;
         try
         {
            e = _parser.Parse<object,T>(expression);
            return e.Compile()(null);
         }
         catch (XPressionException ex)
         {
            Debug.WriteLine("expression: " + expression);
            Debug.WriteLine("compiled  : " + e);
            Debug.WriteLine(ex.FullMessage);
            throw;
         }
      }
      private bool ParsePredicate(string expression, bool? expectedResult = null)
      {
         var result = Parse<bool>(expression);
         if (expectedResult != null)
         {
            Assert.AreEqual(expectedResult.Value,result);
         }
         return result;
      }

      private object Parse(string expression)
      {
         return Parse<object>(expression);
      }

   }
}
