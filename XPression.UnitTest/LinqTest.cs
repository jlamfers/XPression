using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using XPression.Core;
using XPression.UnitTest.Data;
using XPression.LinqToEntities;

namespace XPression.UnitTest
{
   [TestFixture]
   public class LinqTest
   {

      [Test]
      public void MonkeyTest()
      {
         try
         {
            using (var ctx = new TestDbEntities())
            {
               List<Person> result;
               
               result = ctx.People.Where("geo.distance(Address/Location,geography'POINT(-127.89734578345 45.234534534)') gt 0").ToList();
               Assert.IsTrue(result.Count > 0);

               result = ctx.People.Where("geo.distance(Address.Location,geography'POINT(-127.89734578345 45.234534534)')>0", new CompactParser()).ToList();
               Assert.IsTrue(result.Count > 0);

               result = ctx.People.Where("contains(Name,'e')").ToList();
               Assert.IsTrue(result.Count > 0);

               result = ctx.People.Where("contains(Name,'e') eq true or (now() add duration'P10D' gt now())").ToList();
               Assert.IsTrue(result.Count > 0);

               // duration format is recognized bij tokenizer (without explicit type prefixer)
               result = ctx.People.Where("contains(Name,'e') eq true or (now() add P10D gt now())").ToList();
               Assert.IsTrue(result.Count > 0);

               result = ctx.People.Where("Born add duration'P10D' lt date(now())").ToList();
               Assert.IsTrue(result.Count > 0);

               result = ctx.People.Where("CreatedAt add -duration'P10D' lt now()").ToList();
               Assert.IsTrue(result.Count > 0);

               result = ctx.People.Where("CreatedAt+-P10D<now()", new CompactParser()).ToList();
               Assert.IsTrue(result.Count > 0);

               result = ctx.People.Where("Name ne null").ToList();
               Assert.IsTrue(result.Count > 0);

               result = ctx.People.Where("Gender has XPression.UnitTest.Data.Gender'Male'").ToList();
               Assert.IsTrue(result.Count > 0);

               Stopwatch sw;
               sw = new Stopwatch();
               sw.Start();
               for (var i = 0; i < 1000; i++)
               {
                  ctx.People.Where(p => p.Name.Contains("e") || p.Name.CompareTo("a") > 0);
               }
               sw.Stop();
               Debug.WriteLine("1000 ling queries elapsed (ms): " +sw.ElapsedMilliseconds);

               sw = new Stopwatch();
               sw.Start();
               for (var i = 0; i < 1000; i++)
               {
                  ctx.People.Where("contains(Name,'e') or Name gt 'a'");
               }
               sw.Stop();
               Debug.WriteLine("1000 xpression queries elapsed (ms): "+sw.ElapsedMilliseconds);

               sw = new Stopwatch();
               sw.Start();
               for (var i = 0; i < 1000; i++)
               {
                  ctx.People.Where("Name?e|Name>a",new CompactParser());
               }
               sw.Stop();
               Debug.WriteLine("1000 compact xpression queries elapsed (ms): " + sw.ElapsedMilliseconds);


            }
         }
         catch (XPressionException ex)
         {
            Debug.WriteLine(ex.FullMessage);
         }
      }
   }
}
