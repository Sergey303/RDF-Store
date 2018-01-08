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
            if (Directory.Exists(_dataDirectory))
            {
                Directory.Delete(_dataDirectory, true);
            }
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

            var sq1 = SparqlQueryParser.Parse(_store, q1);
            var sq2 = SparqlQueryParser.Parse(_store, q2);
            var sq3 = SparqlQueryParser.Parse(_store, q3);
            var sq4 = SparqlQueryParser.Parse(_store, q4);
            var sq5 = SparqlQueryParser.Parse(_store, q5);
            var sq6 = SparqlQueryParser.Parse(_store, q6);
            var sq7 = SparqlQueryParser.Parse(_store, q7);

            var r1 = sq1.Run().ToCommaSeparatedValues();
            var r2 = sq1.Run().ToCommaSeparatedValues();
            var r3 = sq1.Run().ToCommaSeparatedValues();
            var r4 = sq1.Run().ToCommaSeparatedValues();
            var r5 = sq1.Run().ToCommaSeparatedValues();
            var r6 = sq1.Run().ToCommaSeparatedValues();
            var r7 = sq1.Run().ToCommaSeparatedValues();

            Console.WriteLine(r1);
            Console.WriteLine();
            Console.WriteLine(r2);
            Console.WriteLine();
            Console.WriteLine(r3);
            Console.WriteLine();
            Console.WriteLine(r4);
            Console.WriteLine();
            Console.WriteLine(r5);
            Console.WriteLine();
            Console.WriteLine(r6);
            Console.WriteLine();
            Console.WriteLine(r7);
            Console.WriteLine();
        }

        private static void test(Store store)
        {
            var sparqlQuery = SparqlQueryParser.Parse(store, "Select *  { ?s ?p \"kl\"@ru . Filter( ?s > 0 ) }");

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
