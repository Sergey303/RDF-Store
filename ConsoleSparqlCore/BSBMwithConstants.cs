using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RDFCommon.Interfaces;
using RDFStore;
using SparqlQuery.SparqlClasses;

namespace ConsoleSparqlCore
{
    public static class BSBMwithConstants
    {
        public static void Run()
        {
            double[] memoryUsage = new double[12];
            double[] totalparseMS = new double[12];
            double[] totalCreateLinqStack = new double[12];
            double[] totalrun = new double[12];
            Console.WriteLine("bsbm with constants");
            var timer = new Stopwatch();
            
            // var Store = new Store("../../../Databases/int based/");
            var Store = new RDFRamStore();
            
            //разогрев транслятора и интерпретатора
            SparqlQueryParser.Parse((IStore) Store, BSBMSampleQueries.sq5);

            for (int i = 0; i < 12; i++)
            {
                string file = $@"examples\bsbm\queries\with constants\{i}.rq";
                var readAllText = File.ReadAllText(file);

                GC.Collect();
                timer.Restart();

                var sparqlQuery = SparqlQueryParser.Parse((IStore) Store, readAllText);

                timer.Stop();
                totalparseMS[i] += timer.GetTimeWthLast2Digits();
                timer.Restart();

                var sparqlResultSet = sparqlQuery.Run();

                timer.Stop();
                totalCreateLinqStack[i] += timer.GetTimeWthLast2Digits();
                timer.Restart();

                sparqlResultSet.Results.ToArray();

                timer.Stop();
                totalrun[i] += timer.GetTimeWthLast2Digits();
                memoryUsage[i] = GC.GetTotalMemory(false);

                File.WriteAllText(Path.ChangeExtension(file, ".json"), sparqlResultSet.ToJson());
            }
            using (StreamWriter r = new StreamWriter(@"output.txt", true))
            {
                r.WriteLine("date time " + DateTime.Now);
                r.WriteLine("memory usage (bytes)" + string.Join(", ", memoryUsage));
                r.WriteLine("parse (ms)" + string.Join(", ", totalparseMS));
                r.WriteLine("total parse " + totalparseMS.Sum());
                r.WriteLine("create linq stack " + string.Join(", ", totalCreateLinqStack));
                r.WriteLine("total create linq stack " + totalCreateLinqStack.Sum());
                r.WriteLine("run " + string.Join(", ", totalrun));
                r.WriteLine("parse + create linq stack + run " + string.Join(", ",
                                totalrun.Select((d, i) => d + totalCreateLinqStack[i] + totalparseMS[i])
                                    .Select(d => d.ToString().Replace(",", "."))));
                r.WriteLine("total run " + totalrun.Sum());
                r.WriteLine("total " +
                            (totalparseMS.Sum() + totalCreateLinqStack.Sum() + totalrun.Sum()));
            }
        }
    }
}