﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConsoleEndpoint
{
    using System.Xml.Linq;

    using ConsoleEndpoint.Sparql;
    using ConsoleEndpoint.XML_Serialization;

    using SparqlQuery = ConsoleEndpoint.Sparql.SparqlQuery;

    class Program
    {
        static void Main(string[] args)
        {
            var path = "pacs/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using (Stream table = File.Open(path + "table.pac", FileMode.OpenOrCreate))
            using (Stream index1 = File.Open(path + "spo.pac", FileMode.OpenOrCreate))
            using (Stream index2 = File.Open(path + "ops.pac", FileMode.OpenOrCreate))
            using (Stream stream4 = File.Open(path + "p.pac", FileMode.OpenOrCreate))
            using (Stream ids = File.Open(path + "ids.pac", FileMode.OpenOrCreate))
            using (Stream offsets = File.Open(path + "offsets.pac", FileMode.OpenOrCreate))
            {
                var nametable = new Nametable(ids, offsets);
                Store store = new Store(table, index1, index2, stream4, nametable);
                int nrecords = 1_000_000;
                // codes: 0 - <type>, 1 - <person>, 2 - <name>, 3... persons
                var flow = Enumerable.Range(0, nrecords).SelectMany(
                    i => new[]
                             {
                                 new object[]
                                     {
                                         nametable.GetSetCode((i + 3).ToString()), nametable.GetSetCode(1.ToString()),
                                         new object[] { OVT.iri, nametable.GetSetCode((i + 4).ToString()) }
                                     },
                                 new object[]
                                     {
                                         nametable.GetSetCode((i + 3).ToString()), nametable.GetSetCode(2.ToString()),
                                         new object[] { OVT.@string, "Pupkin" + i }
                                     }
                             }).ToArray();

                T.Restart();
                // store.Load(flow);
                T.Stop();
                Console.WriteLine($"Load of {store.table.Count()} triples OK. Duration={T.ElapsedMilliseconds}");

                T.Restart();
                store.table.Warmup();
                T.Stop();
                Console.WriteLine($"warm up of {store.table.Count()} triples OK. Duration={T.ElapsedMilliseconds}");

                var query = new SparqlSelectQuery(
                    new[] { new SparqlTriple(subjectSource: "555", predicateSource: "1", oVar: "?o") });
                SparqlSelectResultSet sparqlResultSet = query.Run(store);
                Console.WriteLine("sr" + sparqlResultSet.ToCommaSeparatedValues());

                SparqlQuery serializedQuery = new XElement(
                    "SparqlSelectQuery",
                    new XElement(
                        "triple",
                        new XAttribute("sv", "?s"),
                        new XAttribute("pv", "?p"),
                        new XAttribute("o", "500"),
                        new XAttribute("otype", "1"))).Serialize();
                var selectQuery = (SparqlSelectQuery)serializedQuery;
                Console.WriteLine("2 query: " + selectQuery.Run(store).ToCommaSeparatedValues());
                var queryOptional = new SparqlSelectQuery(
                    new ISparqlGraphPattern[]
                        {
                            new SparqlTriple(
                                subjectSource: "12",
                                predicateSource: "1",
                                oVar: "?o"),
                            new Optional(
                                new SparqlTriple(
                                    sVar: "?s",
                                    predicateSource: "2",
                                    oVar: "?o"),
                                new SparqlTriple(
                                    sVar: "?s",
                                    predicateSource: "1",
                                    oVar: "?o"))
                            ,new Optional(new SparqlTriple(
                                sVar: "?s1",
                                predicateSource: "2",
                                oVar: "?o")), 
                        });
                Console.WriteLine("optional query: " + queryOptional.Run(store).ToCommaSeparatedValues());
                var queryUmion = new SparqlSelectQuery(
                    new ISparqlGraphPattern[]
                        {
                            new SparqlTriple(
                                sVar: "?s",
                                predicateSource: "1",
                                objectSource: "435"),
                            new Union(
                                new SparqlTriple(
                                    sVar: "?s",
                                    predicateSource: "1",
                                    oVar: "?o"),
                                new SparqlTriple(
                                    sVar: "?s",
                                    predicateSource: "2",
                                    oVar: "?o")),
                            new SparqlTriple(
                                sVar: "?s",
                                pVar: "?p",
                                oVar: "?o1")
                        });
                Console.WriteLine("union query: " + queryUmion.Run(store).ToCommaSeparatedValues());
            }
        }

            public static void TestStore()
        {
            var path = "pacs/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (Stream table = File.Open(path + "table.pac", FileMode.OpenOrCreate))
            using (Stream index1 = File.Open(path + "spo.pac", FileMode.OpenOrCreate))
            using (Stream index2 = File.Open(path + "ops.pac", FileMode.OpenOrCreate))
            using (Stream stream4 = File.Open(path + "p.pac", FileMode.OpenOrCreate))
            using (Stream ids = File.Open(path + "ids.pac", FileMode.OpenOrCreate))
            using (Stream offsets = File.Open(path + "offsets.pac", FileMode.OpenOrCreate))
            {
                var nametable = new Nametable(ids, offsets);
                Store store = new Store(table, index1, index2, stream4, nametable);
                int nrecords = 1_000_000;
                // codes: 0 - <type>, 1 - <person>, 2 - <name>, 3... persons
                var flow = Enumerable.Range(0, nrecords).SelectMany(
                    i => new[]
                             {
                                 new object []{ nametable.GetSetCode((i + 3).ToString()), nametable.GetSetCode(1.ToString()), new object[]{OVT.iri, nametable.GetSetCode((i + 4).ToString())}},
                                 new object []{nametable.GetSetCode((i + 3).ToString()), nametable.GetSetCode(2.ToString()), new object[]{OVT.@string, "Pupkin" + i}}
                             }).ToArray();

                T.Restart();
                store.Load(flow);
                T.Stop();
                Console.WriteLine($"Load of {nrecords * 2} triples ok. Duration={T.ElapsedMilliseconds}");

                T.Restart();
                store.table.Warmup();
                T.Stop();
                Console.WriteLine($"warm up of {nrecords * 2} triples ok. Duration={T.ElapsedMilliseconds}");

                Console.WriteLine("first 10 SPO");
                foreach (var triple in store.SPO().Take(10))
                {
                    var (s, p, o) = triple.CastRow<int, int, object[]>();
                    Console.Write(" " + s);
                    Console.Write(" " + p);
                    Console.WriteLine(" " + o[1]);
                }

                //Console.WriteLine(query.Count());
                //PType tp_triple = PolarExtension.GetPolarType<IRecord<int, int, IUnion<int, string>>>();
                

                Console.WriteLine("sPO (nrecords * 2 / 3)");
                var query = store.sPO(nrecords * 2 / 3).ToArray();
                foreach (var q in query)
                {
                    //вызывается метод Deconstruct для object[]
                    var(p, ov) = q;
                    Console.WriteLine(p+" "+ov);
                }

                Console.WriteLine("sPO (nrecords * 2 / 3) + 1->50");
                for (int i = 0; i < 50; i++)
                {
                    Console.WriteLine(store.sPO((nrecords * 2 / 3) + i).ToArray().Length);
                }

                int nprobes = 1000;
                Random rnd = new Random();
                T.Restart();
                for (int i = 0; i < nprobes; i++)
                {
                    int code = rnd.Next(nrecords);
                    store.sPO(code ).Count();
                }
                T.Stop();
                Console.WriteLine($"GetTriplesBySubject {nprobes} times. Duration={T.ElapsedMilliseconds}");
            }
        }
        public static Stopwatch T =new Stopwatch();
    }

}
