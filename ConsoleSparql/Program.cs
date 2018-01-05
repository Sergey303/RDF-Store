using System;
using System.IO;
using RDFStore;
using System.Xml.Serialization;
using SparqlQuery.SparqlClasses;

namespace ConsoleSparql
{
    class Program
    {
        static void Main(string[] args)
        {

            //string path = "../../../Databases/Store/";
            //Console.WriteLine("Start SparqlConsole");
            //Console.WriteLine("Creating Store");

            ////Store store = new Store(path);
            //RDFRamStore store = new RDFRamStore();
            //store.ReloadFrom(Config.Source_data_folder_path+"1M.ttl");
            ////store.Start();
           
            SparqlTesting.RunBerlinsWithConstants();

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
