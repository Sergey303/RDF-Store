using System;

namespace ConsoleEndpoint
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            Test();
        }

        public static void Test()
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
                Store store = new Store(table, index1, index2, stream4, new Nametable(ids, offsets));
                int nrecords = 1_000_000;
                // codes: 0 - <type>, 1 - <person>, 2 - <name>, 3... persons
                var flow = Enumerable.Range(0, nrecords).SelectMany(
                    i => new[]
                             {
                                 ((i + 3).ToString(), 0.ToString(), new OV(OVT.iri, (i + 4).ToString())),
                                 ((i + 3).ToString(), 2.ToString(), new OV(OVT.@string, "Pupkin" + i))
                             }).ToArray();

                T.Restart();
                store.Load(flow);
                T.Stop();
                Console.WriteLine($"Load of {nrecords * 2} triples ok. Duration={T.ElapsedMilliseconds}");

                T.Restart();
                store.table.Warmup();
                T.Stop();
                Console.WriteLine($"warm up of {nrecords * 2} triples ok. Duration={T.ElapsedMilliseconds}");

                var query = store.sPO(nrecords * 2 / 3).ToArray();
                //Console.WriteLine(query.Count());
                //PType tp_triple = PolarExtension.GetPolarType<IRecord<int, int, IUnion<int, string>>>();
                for (int i = 0; i < 50; i++)
                {
                    Console.WriteLine(store.sPO((nrecords * 2 / 3)+i).ToArray().Length);
                }
                return;
                foreach (var q in query)
                {
                    var(p, ov) = q;
                    Console.WriteLine(p);
                    Console.WriteLine(ov.OValue);
                    Console.WriteLine();
                }

                int nprobes = 1000;
                Random rnd = new Random();
                T.Restart();
                for (int i = 0; i < nprobes; i++)
                {
                    int code = rnd.Next(nrecords);
                    store.sPO(code).Count();
                }
                T.Stop();
                Console.WriteLine($"GetTriplesBySubject {nprobes} times. Duration={T.ElapsedMilliseconds}");
            }
        }
        public static Stopwatch T =new Stopwatch();
    }
}
