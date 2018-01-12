using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFStore;
using SparqlQuery.SparqlClasses;

namespace ConsoleSparqlCore
{
    class Program
    {
        private static string _dataDirectory;
        private static Store _store;
        private static readonly Stopwatch T = new Stopwatch();
        private static readonly Random Random = new Random();

        static void Main(string[] args)
        {
            string path;

            //Main1(args);
            //  Main2(args);

            Action<INametable> testNT;
            //testNT = TestNameTable;
            testNT = Mag_Nametable.TestNameTable;

            Console.WriteLine("TestMag_Nametable");
            path = "mag_data/";
            using (var stream1 = File.Open(path + "name table ids.pa", FileMode.OpenOrCreate))
            using (var stream2 = File.Open(path + "name table off.pa", FileMode.OpenOrCreate))
            {
                testNT(new Mag_Nametable(stream1, stream2));
            }

            Console.WriteLine("TestNameTableDictionaryRam");
            path = "..\\" + Config.DatabaseFolder;
            Directory.CreateDirectory(path);
            testNT(new NameTableDictionaryRam(path));
            Directory.Delete(path, true);
        }

        private static void TestMag_Nametable()
        {
            //var path = "..\\" + Config.DatabaseFolder;
            //Directory.CreateDirectory(path);
            string path = "mag_data/";
            using (var stream1 = File.Open(path + "name table ids.pa", FileMode.OpenOrCreate))
            using (var stream2 = File.Open(path + "name table off.pa", FileMode.OpenOrCreate))
            {
                TestNameTable(new Mag_Nametable(stream1, stream2));
            }

            //Directory.Delete(path, true);
        }

        private static void TestNameTableDictionaryRam()
        {
            var path = "..\\" + Config.DatabaseFolder;
            Directory.CreateDirectory(path);
            TestNameTable(new NameTableDictionaryRam(path));
            Directory.Delete(path, true);
        }

        static void Main1(string[] args)
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

        public static void Main2(string[] args)
        {
            Random rnd = Random;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Start RDF-Store Main2()");
            string mag_path = "mag_data/";
            string mag_data = mag_path + "mag_data.ttl";
            bool toload = false;
            if (!Directory.Exists(mag_path)) { toload = true; Directory.CreateDirectory(mag_path); }
            FileStream fs = File.Open(mag_data, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            TextWriter tw = new StreamWriter(fs);
            _dataDirectory = mag_path;
            Store stor = new Store(mag_path);
            int nelements = 1_000_000;

            if (toload)
            {
                sw.Restart();
                tw.WriteLine("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .");
                tw.WriteLine("@prefix iis: <http://www.iis.nsk.su/> .");

                for (int i = 0; i < nelements; i++)
                {
                    tw.WriteLine($"<p{i}> ");
                    tw.WriteLine($"\trdf:type <person>;");
                    tw.WriteLine($"\tiis:name \"pupkin{i}\";");
                    tw.WriteLine($"\t<age> \"{(double)(rnd.NextDouble() * 100)}\".");
                }
                tw.Flush();
                fs.Close();
                sw.Stop();
                Console.WriteLine($"data {nelements} build ok. duration={sw.ElapsedMilliseconds}");

                sw.Restart();
                //var stor = new Store(mag_path);
                stor.ReloadFrom(mag_data);
                stor.BuildIndexes();
                sw.Stop();
                Console.WriteLine($"data {nelements} load ok. duration={sw.ElapsedMilliseconds}");
            }
            int nprobe = 100;

            sw.Restart();
            for (int j=0; j<nprobe; j++)
            {
                string squery = "select * { <p"+ rnd.Next(nelements) +"> ?p ?o }";
                var r1 = stor.ParseRunSparql(squery).ToCommaSeparatedValues();
                //Console.WriteLine(r1);
            }
            sw.Stop();
            Console.WriteLine($"{nprobe} selects from {nelements} elements. duration={sw.ElapsedMilliseconds}");
        }

        static void TestNameTable(INametable nt)
        {
           // int startCapacity=1000*1000;
            int allCount = 10*1000 * 1000;//startCapacity+1000;//1001000
            //int firstPortionSize = 1000;
            //int portionSize = 100*1000;
            //int portionsCount = 10;
            int lastPortionAddByGetset = allCount;//1000;

            var allStrings = new List<string>(allCount);

            List<string> RandomStringsList(int count)
            {
                return Enumerable.Repeat(0, count).Select(i => Guid.NewGuid().ToString())
                    .ToList();
            }

            var newstrings = RandomStringsList(lastPortionAddByGetset);
            allStrings.AddRange(newstrings);
            T.Restart();
            foreach (var newstring in newstrings)
            {
                var code = nt.GetSetCode(newstring);
            }
            T.Stop();
            Console.WriteLine($" get set {newstrings.Count} codes for new strings time {T.ElapsedMilliseconds}");


            //get code test
            int getCodeCalls = 1000*1000;
            var randomExistingStrings = Enumerable.Range(0, getCodeCalls)
                .Select(j=>Random.Next(getCodeCalls))
                .Select(randomI => allStrings[randomI])
                .ToList();

            T.Restart();
            for (int j = 0; j < getCodeCalls; j++)
            {
                nt.GetCode(randomExistingStrings[j]);
            }
            T.Stop();
            Console.WriteLine($" get {getCodeCalls} existing codes time {T.ElapsedMilliseconds}");

            var randomNotExistingStrings = RandomStringsList(getCodeCalls);

            T.Restart();
            for (int j = 0; j < getCodeCalls; j++)
            {
                nt.GetCode(randomNotExistingStrings[j]);
            }
            T.Stop();
            Console.WriteLine($" get {getCodeCalls} not existing codes time {T.ElapsedMilliseconds}");
          

            //get string test
            //int getStringCalls = 1000 * 1000;
            var randomExistingCodes = Enumerable.Range(0, getCodeCalls)
                .Select(j => Random.Next(getCodeCalls))
                .Select(randomI => allStrings[randomI])
                .Select(nt.GetCode)
                .ToList();

            T.Restart();
            for (int j = 0; j < getCodeCalls; j++)
            {
                nt.GetCode(randomExistingStrings[j]);
            }
            T.Stop();
            Console.WriteLine($" get {getCodeCalls} existing strings time {T.ElapsedMilliseconds}");

            }
    }
}
