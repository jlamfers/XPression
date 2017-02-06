using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using XPression.Core;
using XPression.Core.ShuntingYard;
using XPression.Language;
using XPression.Language.Syntax;

namespace XPression.UnitTest
{
   [TestFixture]
   public class Demo
   {
      [Test]
      public void Demos()
      {
         var parser = new QueryParser { Grammar = { Strict = true } };

         parser.Compile("1d / 2 + 3");
         var f = parser.Compile("a / b + c % @null", new { a = 2d, b = 3, c = 4, @null = 2 });

         Debug.WriteLine(f(new { a = 3d, b = 4, c = 5, @null = 2 }));

         var e = parser.Parse("a / b + c % @null", new { a = 2d, b = 3, c = 4, @null = 2 });
         f = e.Compile();
         Debug.WriteLine(e.ToString());

         var result = parser.Compile("null / 1")();

         var e2 = parser
            .WithReturnType<double>()
            .Parse("a / b + c % @null", new { a = 2d, b = 3, c = 4, @null = 2 });

         var f2 = e2.Compile();

         f2(new { a = 3d, b = 4, c = 5, @null = 6 });

      }

      [Test]
      public void PerformanceTest()
      {
         // this test shows that 
         var grammar = new Grammar(new QuerySyntax());
         var lexer = new Lexer(grammar);
         var parser = new Parser(grammar);
         var source = "(1+3)*(5.0/0.4)-16.3e5";
         //var source = "(color=white or color=green) and wheels >=10";
         var shuntingYard = new ShuntingYardParser(grammar);

         var tokens = lexer.Tokenize(source).ToArray();
         var rpn = shuntingYard.BuildRPN(tokens);
         var exp = shuntingYard.BuildAST<object, double>(tokens);
         var expression = parser.Parse<object, double>(source);
         var d = expression.Compile();

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            lexer.Tokenize(source);
         }
         sw.Stop();
         Debug.WriteLine("tokenizing: " + sw.ElapsedMilliseconds);

         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            shuntingYard.BuildRPN(tokens);
         }
         sw.Stop();
         Debug.WriteLine("infix->postfix: " + sw.ElapsedMilliseconds);

         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            shuntingYard.BuildAST<object, double>(tokens);
         }
         sw.Stop();
         Debug.WriteLine("infix->AST: " + sw.ElapsedMilliseconds);

         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            parser.Parse<object, double>(source);
         }
         sw.Stop();
         Debug.WriteLine("source->ast: " + sw.ElapsedMilliseconds);

         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 10000; i++)
         {
            expression.Compile();
         }
         sw.Stop();
         Debug.WriteLine("ast->IL: " + sw.ElapsedMilliseconds);


      }

      public class Address
      {
         public string Street { get; set; }
         public string Number { get; set; }
         public string City { get; set; }
      }
      public class Person
      {
         public Guid? Id { get; set; }
         public string FirstName { get; set; }
         public string LastName { get; set; }
         public DateTime BornAt { get; set; }
         public Address Address { get; set; }

      }
      [Test]
      public void PerformanceTest2()
      {
         var grammar = new Grammar(new QuerySyntax()) { Strict = true };
         var lexer = new Lexer(grammar);
         var parser = new Parser(grammar);
         var source = "Id!=null and FirstName like '%e%' and LastName like '%e%' and BornAt<date(now()) and Address.Street like 'e%' and Address.City like '%e' or Address.Number like '%0%'";
         var shuntingYard = new ShuntingYardParser(grammar);

         var tokens = lexer.Tokenize(source).ToList();
         var rpn = shuntingYard.BuildRPN(tokens);
         var exp = shuntingYard.BuildAST<Person, bool>(tokens);
         Debug.WriteLine(exp);
         var expression = parser.Parse<Person, bool>(source);
         var d = expression.Compile();

         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            tokens = lexer.Tokenize(source).ToList();
         }
         sw.Stop();
         Debug.WriteLine("source->tokens: " + sw.ElapsedMilliseconds);
         sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1000; i++)
         {
            shuntingYard.BuildAST<Person, bool>(tokens);
         }
         sw.Stop();
         Debug.WriteLine("infix->AST: " + sw.ElapsedMilliseconds);


      }

      [Test]
      public void ODataDemo()
      {
         Parser parser = new ODataParser();
         var expr1 = parser.Parse<Person,bool>("contains(Address/City,'ams') and FirstName gt 'm'");

         // same query, using another syntax, and so using another parser
         parser = new CompactParser();
         var expr2 = parser.Parse<Person,bool>("Address.City?ams&FirstName>m");

         Assert.AreEqual(expr1.ToString(),expr2.ToString());

         parser = new Parser(new Grammar(ScriptSyntax<DefaultSyntaxExtender>.Instance));
         var args = new {a = 10, b = DateTime.Now, c = "foo", d = 10.2};
         var expr3 = parser.WithReturnType<double>().Parse("a + day(b) + length(c) + d/2", args);
         var fn = expr3.Compile();
         double result = fn(args);
         Debug.WriteLine(result);
      }

      [Test]
      public void OrderTest()
      {
         var exp = OrderExpression.Parse<Person>("firstname,-lastname,address.city,address.street desc");
         foreach (var e in exp)
         {
            Debug.Write(e.Item1);
            Debug.WriteLine(", descending order: "+e.Item2);
         }
      }

   }
}
