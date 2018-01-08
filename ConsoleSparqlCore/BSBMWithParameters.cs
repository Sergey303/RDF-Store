using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFStore;
using SparqlQuery.SparqlClasses;

namespace ConsoleSparqlCore
{
    public static partial class BSBMWithParameters
    {
        public static void RunBerlinsParameters()
        {
            var Store = new Store("../../../Databases/int based/");
            //Store.ReloadFrom(Config.Source_data_folder_path + "1.ttl");
            Store.Start();
            
            //Store.Start();
            //Store.Warmup();
            Console.WriteLine("bsbm parametered");
            var paramvaluesFilePath = string.Format(@"..\..\examples\bsbm\queries\parameters\param values for{0} m.txt", 1);
            //            using (StreamWriter streamQueryParameters = new StreamWriter(paramvaluesFilePath))
            //                for (int j = 0; j < 1000; j++)
            //                    foreach (var file in fileInfos.Select(info => File.ReadAllText(info.FullName)))
            //                        QueryWriteParameters(file, streamQueryParameters, ts);
            //return;
     
            using (StreamReader streamQueryParameters = new StreamReader(paramvaluesFilePath))
            {
                for (int j = 0; j < 500; j++)
                    for (int i = 1; i < 13; i++)
                        BSBmParams.QueryReadParameters(File.ReadAllText(string.Format(@"..\..\examples\bsbm\queries\parameters\{0}.rq", i)),
                            streamQueryParameters);

                SubTestRun(streamQueryParameters, 500, Store);
            }
        }

        private static void SubTestRun(StreamReader streamQueryParameters, int i1, IStore Store)
        {
            double[] results = new double[12];
            double[] minimums = Enumerable.Repeat(double.MaxValue, 12).ToArray();
            double[] maximums = new double[12];
            double maxMemoryUsage = 0;
            double[] totalparseMS = new double[12];
            double[] totalrun = new double[12];
            for (int j = 0; j < i1; j++)
            {
                for (int i = 0; i < 12; i++)
                {
                    string file = string.Format(@"..\..\examples\bsbm\queries\parameters\{0}.rq", i+1);
                    var readAllText = File.ReadAllText(file);
                    readAllText = BSBmParams.QueryReadParameters(readAllText, streamQueryParameters);

                    var timer = new Stopwatch();
                    SparqlQuery.SparqlClasses.Query.SparqlQuery sparqlQuery = null;
                  //  if (i == 0)
                    {
                        sparqlQuery = SparqlQueryParser.ParseSparql(Store, readAllText);
                    }

                    totalparseMS[i] += timer.GetTimeWthLast2Digits();
                    var st1 = DateTime.Now;
                    //if (i == 0)
                    {
                        var sparqlResultSet = sparqlQuery.Run().ToJson();                        
                    }
                    totalrun[i] += (DateTime.Now - st1).Ticks / 10000L;
                    var totalMilliseconds = timer.GetTimeWthLast2Digits();

                    var memoryUsage = GC.GetTotalMemory(false);
                    if (memoryUsage > maxMemoryUsage)
                        maxMemoryUsage = memoryUsage;
                    if (minimums[i] > totalMilliseconds)
                        minimums[i] = totalMilliseconds;
                    if (maximums[i] < totalMilliseconds)
                        maximums[i] = totalMilliseconds;
                    results[i] += totalMilliseconds;
                    //  File.WriteAllText(Path.ChangeExtension(file.FullName, ".txt"), resultString);
                    //.Save(Path.ChangeExtension(file.FullName,".xml"));
                }
            }
            using (StreamWriter r = new StreamWriter(@"..\..\output.txt", true))
            {
                r.WriteLine("milions " + 1);
                r.WriteLine("date time " + DateTime.Now);
                r.WriteLine("max memory usage " + maxMemoryUsage);
                r.WriteLine("average " + string.Join(", ", results.Select(l => l == 0 ? "inf" : (500 * 1000 / l).ToString())));
                r.WriteLine("minimums " + string.Join(", ", minimums));
                r.WriteLine("maximums " + string.Join(", ", maximums));
                r.WriteLine("total parse " + string.Join(", ", totalparseMS));
                r.WriteLine("total run " + string.Join(", ", totalrun));
                r.WriteLine("total " + totalparseMS.Sum()+totalrun.Sum());
                //    r.WriteLine("countCodingUsages {0} totalMillisecondsCodingUsages {1}", TripleInt.EntitiesCodeCache.Count, TripleInt.totalMilisecondsCodingUsages);

                //r.WriteLine("EWT average search" + EntitiesMemoryHashTable.total / EntitiesMemoryHashTable.count);
                //r.WriteLine("EWT average range" + EntitiesMemoryHashTable.totalRange / EntitiesMemoryHashTable.count);  
            }
        }

        public static void RunTestParametred(int count = 100)
        {

            var Store = new Store("../../../Databases/int based/");
            Store.ReloadFrom(Config.TurtleFileFullPath);
            SparqlQueryParser.ParseSparql((IStore) Store, BSBMSampleQueries.sq5);
            //Store.Start();
            //Store.Warmup();
            for (int i = 0; i < 12; i++)
            {

                BSBmParams bsBmParams = new BSBmParams(Store);

                var paramvaluesFilePath =
                    string.Format(@"..\..\examples\bsbm\queries\parameters\param values for{0}m {1} query.txt", 1, i + 1);
                var qFile =
                    string.Format(@"..\..\examples\bsbm\queries\parameters\{0}.rq", i+1);
                using (StreamReader streamParameters = new StreamReader(paramvaluesFilePath))
                using (StreamReader streamQuery = new StreamReader(qFile))
                {
                    string qparams = streamQuery.ReadToEnd();
                    Stopwatch timer = new Stopwatch();
                    for (int j = 0; j < count; j++)
                    {
                        string q = BSBmParams.QueryReadParameters(qparams, streamParameters);
                        var sparqlResults = SparqlQueryParser.ParseSparql((IStore) Store, q).Run();

                        timer.Start();
                        sparqlResults.Results.Count();
                        timer.Stop();
                    }

                    using (StreamWriter r = new StreamWriter(@"..\..\output.txt", true))
                    {
                        r.WriteLine();
                        r.WriteLine("one query {0}, {1} times", i+1, count);
                        r.WriteLine("milions " + 1);
                        r.WriteLine("date time " + DateTime.Now);
                        r.WriteLine("total ms " + timer.ElapsedMilliseconds);
                        double l = ((double) timer.ElapsedMilliseconds)/count;
                        r.WriteLine("ms " + l);

                        r.WriteLine("qps " + (int) (1000.0/l));
                        string q = BSBmParams.QueryReadParameters(qparams, streamParameters);
                        r.WriteLine("next results count: {0}",
                            SparqlQueryParser.ParseSparql((IStore) Store, q).Run().Results.Count());
                    }
                }
            }
        }

        public static void OneBerlinParametrized(IStore store, int i, int count)
        {

          SparqlQueryParser.ParseSparql(store, BSBMSampleQueries.sq5);      
           store.Warmup();
            using (StreamReader streamQueryParameters = new StreamReader(string.Format(
                @"..\..\..\Testing\examples\bsbm\queries\parameters\param values for{0}m {1} query.txt", 1, i)))
            {
                var file =
                    new FileInfo(string.Format(@"..\..\..\Testing\examples\bsbm\queries\parameters\{0}.rq", i));
                var parametred = File.ReadAllText(file.FullName);
                double min = int.MaxValue, max = -1, average = 0;
                double averageParse = 0;
                double averageCLS = 0;
                double averageRun = 0;
                for (int j = 0; j < count; j++)
                {
                    var consted = BSBmParams.QueryReadNewParameters(parametred, streamQueryParameters);


                    Stopwatch timer = new Stopwatch();
                    timer.Restart();
                    var sparqlQuery = SparqlQueryParser.ParseSparql(store, consted);
                    timer.Stop();

                    double time1 = timer.GetTimeWthLast2Digits();
                    averageParse += (double) ((int) (100*time1/count))/100;

                    timer.Restart();
                    var sparqlResultSet = sparqlQuery.Run();
                    timer.Stop();

                    double time2 = timer.GetTimeWthLast2Digits();
                    averageCLS += (double) ((int) (100*time2/count))/100;

                    timer.Restart();
                    sparqlResultSet.Results.ToArray();
                    timer.Stop();

                    double time3 = timer.GetTimeWthLast2Digits();
                    averageRun += (double) ((int) (100*time3/count))/100;       
                    
                    var time = time1 + time2 + time3;         
                    average += (double) ((int) (100*time))/100;


                    if (time > max) max = time;
                    if (min > time) min = time;
                }
                using (StreamWriter r = new StreamWriter(@"..\..\output.txt", true))
                {
                    r.WriteLine(DateTimeOffset.Now);
                    r.WriteLine("q " + i);
                    r.WriteLine("average " + average/count);
                    r.WriteLine("qps " + ((double)((int)(100000 * count / average)) / 100));
                    r.WriteLine("min " + min);
                    r.WriteLine("max " + max);
                    r.WriteLine("memory usage (bytes)" + GC.GetTotalMemory(false));
                    r.WriteLine("parse (ms)" + averageParse);
                    r.WriteLine("create linq stack " + averageCLS);
                    r.WriteLine("run " + averageRun);
                }
            }
        }
       public static void OneLUMB(IStore store, int i, int count)
       {

           SparqlQueryParser.ParseSparql(store, BSBMSampleQueries.sq5);
           store.Warmup();
           {
               var file = new FileInfo(string.Format(@"..\..\..\Testing\examples\lubm\q ({0}).rq", i));
               var q = File.ReadAllText(file.FullName);
               double min = int.MaxValue, max = -1, average = 0;
               double averageParse = 0;
               double averageCLS = 0;
               double averageRun = 0;
               for (int j = 0; j < count; j++)
               {         
                   Stopwatch timer = new Stopwatch();
                   timer.Restart();
                   var sparqlQuery = SparqlQueryParser.ParseSparql(store, q);
                   timer.Stop();

                   double time1 = timer.GetTimeWthLast2Digits();
                   averageParse += (double)((int)(100 * time1 / count)) / 100;

                   timer.Restart();
                   var sparqlResultSet = sparqlQuery.Run();
                   timer.Stop();

                   double time2 = timer.GetTimeWthLast2Digits();
                   averageCLS += (double)((int)(100 * time2 / count)) / 100;

                   timer.Restart();
                   sparqlResultSet.Results.ToArray();
                   timer.Stop();

                   double time3 = timer.GetTimeWthLast2Digits();
                   averageRun += (double)((int)(100 * time3 / count)) / 100;

                   var time = time1 + time2 + time3;
                   average += (double)((int)(100 * time)) / 100;


                   if (time > max) max = time;
                   if (min > time) min = time;

                   File.WriteAllText(@"..\..\..\Testing\examples\lubm\q ({0}).json", sparqlResultSet.ToJson());
               }
               using (StreamWriter r = new StreamWriter(@"..\..\output.txt", true))
               {
                   r.WriteLine(DateTimeOffset.Now);
                   r.WriteLine("q " + i);
                   r.WriteLine("average " + average / count);
                   r.WriteLine("qps " + ((double)((int)(100000 * count / average)) / 100));
                   r.WriteLine("min " + min);
                   r.WriteLine("max " + max);
                   r.WriteLine("memory usage (bytes)" + GC.GetTotalMemory(false));
                   r.WriteLine("parse (ms)" + averageParse);
                   r.WriteLine("create linq stack " + averageCLS);
                   r.WriteLine("run " + averageRun);
               }
           }
       }
        
    }
}