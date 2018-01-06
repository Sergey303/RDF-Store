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
        static void Main(string[] args)
        {
            Config.Load(args);
         
            BSBMwithConstants.Run();

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
