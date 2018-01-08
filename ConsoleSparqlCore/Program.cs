using System;
using System.IO;
using System.Xml.Serialization;
using RDFCommon;
using RDFStore;
using SparqlQuery.SparqlClasses;

namespace ConsoleSparqlCore
{
    class Program
    {
        private static string _dataDirectory;
        private static Store _store;

        static void Main(string[] args)
        {
            //Config.Load(args);
            Console.WriteLine(Path.GetFullPath("..\\"));
            var turtleFilePath = @"..\" + Config.TurtleFileFullPath;
            _dataDirectory = @"..\" + Config.DatabaseFolder;
            // _store = new RDFRamStore(); 
            _store = new Store(_dataDirectory);
            _store.ReloadFrom(turtleFilePath);
            ((Store)_store).BuildIndexes();


            RunTests();
            
            //if (Directory.Exists(_dataDirectory))
            //{
            //    Directory.Delete(_dataDirectory, true);
            //}
        }

        private static void RunTests()
        {
            string q1 = @"Select * {?s ?p ?o}";

            string q2 = @"Select * {?s ?p ""Россия""}";
            string q3 = @"Select * {<id1> ?p ?o}";
            string q4 = @"Select * {?s <name> ""Россия""}";

            string q5 = @"Select * {?s <name> ""Россия""}";
            string q6 = @"Select * {<id1> ?p ""Россия""}";
            string q7 = @"Select * {<id1> <name> ?o}";

            string q8 = @"Select * 
{?org <name> ""Россия"" ;
      <type> <contry> . 
?pat   <in-org> ?org ;
        <participant> ?person .
?person <name> ?name ;
        <type> <person> . }";
          //  string q9 = @"Select * {<id1> <name> ?o}";


            Console.WriteLine(q1);
            Console.WriteLine();
            var r1 = _store.ParseRunSparql(q1).ToCommaSeparatedValues();
            Console.WriteLine(r1);
            Console.WriteLine();
            Console.WriteLine(q2);
            Console.WriteLine();
            var r2 = _store.ParseRunSparql(q2).ToCommaSeparatedValues();
            Console.WriteLine(r2);
            Console.WriteLine();
            Console.WriteLine(q3);
            Console.WriteLine();
            var r3 = _store.ParseRunSparql(q3).ToCommaSeparatedValues();
            Console.WriteLine(r3);
            Console.WriteLine();
            Console.WriteLine(q4);
            Console.WriteLine();
            var r4 = _store.ParseRunSparql(q4).ToCommaSeparatedValues();
            Console.WriteLine(r4);
            Console.WriteLine();
            Console.WriteLine(q5);
            Console.WriteLine();
            var r5 = _store.ParseRunSparql(q5).ToCommaSeparatedValues();
            Console.WriteLine(r5);
            Console.WriteLine();
            Console.WriteLine(q6);
            Console.WriteLine();
            var r6 = _store.ParseRunSparql(q6).ToCommaSeparatedValues();
            Console.WriteLine(r6);
            Console.WriteLine();
            Console.WriteLine(q7);
            Console.WriteLine();
            var r7 = _store.ParseRunSparql(q7).ToCommaSeparatedValues();            
            Console.WriteLine(r7);
            Console.WriteLine();
            Console.WriteLine(q8);
            Console.WriteLine();
            var r8 = _store.ParseRunSparql(q8).ToCommaSeparatedValues();
            Console.WriteLine(r8);
            Console.WriteLine();
        }

        private static void test(Store store)
        {
            var sparqlQuery = SparqlQueryParser.ParseSparql(store, "Select *  { ?s ?p \"kl\"@ru . Filter( ?s > 0 ) }");

            Console.WriteLine("parse ok");

            var res = sparqlQuery.Run();

            Console.WriteLine(res.ToCommaSeparatedValues());

            using (var stream = new StreamWriter("../../q.xml"))
            {
                var formatter = new XmlSerializer(typeof(SparqlQuery.SparqlClasses.Query.SparqlQuery));
                // var formatter=new BinaryFormatter();
                formatter.Serialize(stream.BaseStream, sparqlQuery);
            }
        }
    }
}
