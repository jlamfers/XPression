using System;
using System.Diagnostics;
using NUnit.Framework;
using XPression.Core;
using XPression.Language.Syntax;

namespace XPression.UnitTest
{
   public class SyntaxExtension
   {
      public object Result;

      [ScriptMethod("return")]
      public static bool Return()
      {
         // any function returning false => exit script
         return false;
      }
      [ScriptMethod("return")]
      public bool Return(object result)
      {
         Result = result;
         // any function returning false => exit script
         return false;
      }

   }

   [TestFixture]
   public class ScriptTest
   {

      [Test]
      public void ExecuteSimpleScript()
      {
         
         var script = @"
var myjson = {
   'blabla':'oops',
   'yep':'yep',
   'format':{
      'text'='{{'
   }
};
var a = 10;
var b = 2017-01-02; #recognized as date type
var b2 = @2017-01-02; #evaluated as 2017 sub 1 sub 2

var c = day(b) + a;

if(c > 10, return (c), return (-1));

#the following works as well:
if(c > 10, (
      return(c) #note that return is a custom function, defined in the SyntaxExtension
   ); 
   else(
      return (-1) 
   );
);

";
         var parser = new ScriptParser<SyntaxExtension>(strict:true);
         var context = new SyntaxExtension();
         var fn = parser.Compile<SyntaxExtension, int>(script);
         fn(context);
         Assert.AreEqual(12,context.Result);

         // the following works because the context still is around, and strict is true 
         // (strict: any identifier that that cannot be resolved as a member nor variable declaration is assumed to be a late-context-bound variable, it is attempted to be resolved during execution)
         // (not strict: any identifier that that cannot be resolved as a member nor variable declaration is assumed to be a string value)
         var script2 = "return(a);";
         fn = parser.Compile<SyntaxExtension, int>(script2);
         fn(context);
         Assert.AreEqual(10, context.Result);

         fn = parser.Compile<SyntaxExtension, int>("return(b2)");
         fn(context);
         Assert.AreEqual(2017 - 1 - 2, context.Result);


         // now lets create a  new context
         context = new SyntaxExtension();
         try
         {
            // we expect an exception here, because the variable belongs to another context
            fn(context);
            throw new Exception("Exception expected");
         }
         catch (XPressionException ex)
         {
            Debug.WriteLine(ex.FullMessage);
         }

         // non strict (= default) example
         parser = new ScriptParser<SyntaxExtension>(strict: false);
         context = new SyntaxExtension();
         fn = parser.Compile<SyntaxExtension, int>("return(a)");
         fn(context);
         Assert.AreEqual("a", context.Result);
         // non strict is convenient in url queries, where using quotes often is annoying
      }


   }
}
