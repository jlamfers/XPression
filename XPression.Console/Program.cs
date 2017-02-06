﻿#region  License
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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;
using XPression.Core;
using XPression.LinqToEntities;
using XPression.UnitTest.Data;
using con=System.Console;

namespace XPression.Console
{
   /// <summary>
   /// Commandline utility for interactive testing expressions
   /// and tracing sql queries (generated by LINQ)
   /// </summary>
   class Program
   {
      static int Main(string[] args)
      {
         SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

         //const string ConnectionString = "Initial Catalog=MiniProf;Data Source=.\\SqlExpress;Integrated Security=true;";
         //using(var conn = new SqlConnection(ConnectionString))
         //{
         //    conn.Open();
         //    conn.Execute(SqlServerStorage.TableCreationScript);
         //}
         //MiniProfiler.Settings.Storage = new SqlServerStorage(ConnectionString);
         MiniProfiler.Settings.ProfilerProvider = new SingletonProfilerProvider();
         MiniProfiler.Settings.ProfilerProvider.Start(ProfileLevel.Info);
         MiniProfiler.Settings.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
         MiniProfilerEF6.Initialize();
         MiniProfiler.Start();

         Parser parser = new CompactParser();
         var debug = true;
         Type entityType = null;

         con.WriteLine("*** type help for command list ***");

         while (true)
         {
            if (entityType != null)
               System.Console.Write("[enter query for entity '"+entityType.Name + "'] : ");
            else
               System.Console.Write(": ");
            var input = System.Console.ReadLine();
            switch (input)
            {
               case "quit":
               case "exit":
                  return 0;
               case "cls":
               case "clr":
                  System.Console.Clear();
                  continue;
               case "strict":
                  parser.Grammar.Strict = !parser.Grammar.Strict;
                  System.Console.WriteLine("strict is: " + parser.Grammar.Strict);
                  break;
               case "entity":
                  System.Console.Write("[use entity type] : ");
                  var typeName = System.Console.ReadLine();
                  if (string.IsNullOrEmpty(typeName))
                  {
                     entityType = null;
                     break;
                  }
                  entityType = Type.GetType(typeof(Address).AssemblyQualifiedName.Replace("Address", typeName), false, true);
                  if (entityType == null)
                  {
                     con.WriteLine("Type " + typeName + " not found, use Address or Person");
                  }
                  break;
               case "debug":
                  debug = !debug;
                  System.Console.WriteLine("debug is: " + debug);
                  break;
               case "functions":
                  foreach (var m in parser.Grammar.Functions.Select(m=>m.Name).Distinct().OrderBy(s => s))
                  {
                     System.Console.WriteLine(m);
                  }
                  break;
               case "?":
               case "help":
                  System.Console.WriteLine("Commands:");
                  System.Console.WriteLine(" quit       - quit program");
                  System.Console.WriteLine(" cls        - clear screen");
                  System.Console.WriteLine(" strict     - toggle strict on/off");
                  System.Console.WriteLine(" debug      - toggle debug on/off");
                  System.Console.WriteLine(" syntax     - change syntax");
                  System.Console.WriteLine(" functions  - show functions");
                  System.Console.WriteLine(" entity     - set/change entity type as a LINQ query target");
                  System.Console.WriteLine(" <other>    - expression that will be evaluated");
                  break;
               case "syntax":
                  System.Console.Write("[change syntax into] : ");
                  switch (System.Console.ReadLine())
                  {
                     case "odata":
                        parser = new ODataParser();
                        break;
                     case "compact":
                        parser = new CompactParser();
                        break;
                     case "query":
                        parser = new QueryParser();
                        break;
                     case "math":
                        parser = new MathParser();
                        break;
                     default:
                        System.Console.WriteLine("* Unknown syntax. Known syntax types: odata,compact,query,math");
                        break;
                  }
                  break;
               default:
                  if (entityType != null)
                  {
                     if (!string.IsNullOrEmpty(input))
                     {
                        var result = ExpressionToLinqExecuter.Execute(parser, entityType, input);
                        con.WriteLine("* Count: " + result.Count());
                        MiniProfiler.Settings.ProfilerProvider.Stop(false);
                        WriteExecutedQueries();
                        //MiniProfiler.Settings.Storage.Save(MiniProfiler.Current);
                        MiniProfiler.Settings.ProfilerProvider.Start(ProfileLevel.Info);
                     }
                     else
                     {
                        entityType = null;
                     }
                     break;
                  }
                  try
                  {
                     var lambda = parser.Parse<object, object>(input);
                     var @delegate = lambda.Compile();
                     var result = @delegate(new object());
                     if (debug)
                     {
                        System.Console.WriteLine("? {0} => {1}", lambda, lambda.Simplify());
                     }
                     System.Console.WriteLine("= {0}", result ?? "NULL");
                  }
                  catch (XPressionException ex)
                  {
                     System.Console.WriteLine(ex.FullMessage);
                  }
                  catch (Exception ex)
                  {
                     System.Console.WriteLine(ex.Message);
                  }
                  break;
            }

         }

      }

      private static void WriteExecutedQueries()
      {
         var sb = new StringBuilder();

         var profiling = MiniProfiler.Current;
         if (profiling != null)
         {
            var timings = profiling.GetTimingHierarchy();

            foreach (var timing in timings)
            {
               sb.AppendLine(Environment.NewLine + "----START QUERY----");
               try
               {
                  var json = (JObject) JsonConvert.DeserializeObject(timing.CustomTimingsJson);
                  var sql = json["sql"][0]["CommandString"].Value<string>();
                  sql = sql.Replace("\\r\\n", "\r\n");
                  sb.AppendLine(sql);
               }
               catch(Exception ex)
               {
                  sb.AppendLine(ex.Message);
               }
               sb.AppendLine("----END QUERY----");
            }
         }

         con.WriteLine(sb);

      }

      public static class ExpressionToLinqExecuter
      {
         public static IList<object> Execute(Parser parser, Type entityType, string expression)
         {
            var executer = (IExpressionToLinqExecuter)Activator.CreateInstance(typeof(ExpressionToLinqExecuter<>).MakeGenericType(entityType));
            return executer.Execute(parser, expression);
         }
      }

      public interface IExpressionToLinqExecuter
      {
         IList<object> Execute(Parser parser, string expression);
      }

      public class ExpressionToLinqExecuter<TEntity> : IExpressionToLinqExecuter where TEntity : class
      {
         public IList<object> Execute(Parser parser, string expression)
         {
            return _Execute(parser, expression).Cast<object>().ToList();
         }

         private IEnumerable<TEntity> _Execute(Parser parser, string expression)
         {

            IList<TEntity> result = new TEntity[0];
            Expression<Func<TEntity, bool>> lambda;
            try
            {
               lambda = parser.Parse<TEntity, bool>(expression)
                  .Simplify()
                  .AsLinqToEntities();

               con.WriteLine("LINQ: " + lambda);
            }
            catch (XPressionException ex)
            {
               System.Console.WriteLine(ex.FullMessage);
               return result;
            }
            catch (Exception ex)
            {
               System.Console.WriteLine(ex.Message);
               return result;
            }

            using (var ctx = new TestDbEntities())
            {
               try
               {
                  var set =
                     (DbSet<TEntity>)
                        ctx.GetType()
                           .GetProperties()
                           .Single(p => p.PropertyType == typeof(DbSet<TEntity>))
                           .GetValue(ctx);
                  result = set.Where(lambda).ToList();
               }
               catch (Exception ex)
               {
                  System.Console.WriteLine((ex.InnerException ?? ex).Message);
                  return result;
               }
            }

            return result;
         }

      }


   }
}