# Using XPression

As you probably expect, with `XPression` you parse textual expressions and compile these into a syntax tree. The resulting expression tree is a `LINQ Expression` tree, so the final syntax tree is built up from LINQ Expression types. 

XPression lets you define your own syntax, and also XPression includes several ready to use syntaxes. It includes among others the OData $filter syntax.

So with XPression you can define and apply textual LINQ queries using your own syntax or grammar. The only limitation depends on the corresponding LINQ provider itself. 

Also you can define and apply your own script language, even though the result syntax tree is a LINQ Expression tree (which does not not support expression lists and variable assignments by itself).

Now how would you use `XPression`?

```cs
    Parser parser = new ODataParser();
    var expr1 = parser.Parse<Person>("contains(Address/City,'ams') and FirstName gt 'm'");

    // same query, using another syntax, and so using another parser
    parser = new CompactParser();
    var expr2 = parser.Parse<Person>("Address.City?ams&FirstName>m");

    Assert.AreEqual(expr1.ToString(),expr2.ToString());

    parser = new Parser(new Grammar(ScriptSyntax<DefaultSyntaxExtender>.Instance));
    var args = new {a = 10, b = DateTime.Now, c = "foo", d = 10.2};

    // the 'args' argument only is needed her because we cannot pass the generic 
    // type argument. Any anonymous generic type only can be passed implicitly, 
    // e.g., by its argument
    var expr3 = parser.WithReturnType<double>().Parse("a + day(b) + length(c) + d/2", args);

    var fn = expr3.Compile();
    double result = fn(args);
    Debug.WriteLine(result);
```

# How the parser works

The parse process works as follows:
```
                                     +--------+
                                +--->| grammar|<------+
                                |    +--------+       |
                                |                     |
                                |                     |
+-----------+              +--------+            +--------+       +--------------+
| scanner / |              |        |            |        |       |              |
| tokenizer |->[s-tokens]->| lexer  |->[tokens]->| parser |->AST->| (Normalizer) |->AST->(Compile)
|           |              |        |            |        |       |              | 
+-----------+              +--------+            +--------+       +--------------+
```
* The scanner/tokenizer recognizes literals and identifiers. It recognizes Time, Datetime, 
  Duration, String, (Hex)Numerics, Guids and also JSON structures
* The lexer knowns about structure/grammar, produces more diversive tokens, and adds semantics to the tokens
* The parser uses shunting yard and produces the abstract syntax tree (e.g. the LINQ expression tree)
  It needs the grammar to classify any token type (like is it an operator?)
* The normalizer is optional. For example it is needed for linq-to-entities. In that case the
  normalizer substitutes methods that cannot be interpreted by linq-to-entities, by
  methods that linq-to-entities understands. An example for this is DateTime.Add, which must
  be replaced by some DbFunctions.Add....() methods
* Compiling is optional. It simply invokes the Linq expression compiler.


