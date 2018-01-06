using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFStore;
using SparqlQuery.SparqlClasses;

namespace ConsoleSparql
{
    public static class SparqlTesting
    {
  

     
        //public static void InterpretMeas<T>(T Store, int i) where T:InterpretMeasure,IStore
        //{
        //    Store.TrainingMode = true;

        //    using (StreamWriter sr = new StreamWriter(@"..\..\output.txt", true))
        //        sr.WriteLine("train");
        //    SparqlTesting.OneParametrized(Store, i, 100);


        //    using (StreamWriter sr = new StreamWriter(@"..\..\output.txt", true))
        //        sr.WriteLine("history count " + Store.history.Count);
            
        //    Store.TrainingMode = false;
        //    SparqlTesting.OneParametrized(Store, i, 100);

        //}

        //public static void CallsAnalyze<T>(T Store, int i, int count=100)      where T: CacheMeasure,IStore
        //{
        //    Store.TrainingMode = true;
        //    using (StreamReader streamQueryParameters = new StreamReader(string.Format(
        //        @"..\..\..\Testing\examples\bsbm\queries\parameters\param values for{0}m {1} query.txt", 1, i)))
        //    {
        //        var file = new FileInfo(string.Format(@"..\..\..\Testing\examples\bsbm\queries\parameters\{0}.rq", i));
        //        var parametred = File.ReadAllText(file.FullName);
        //         for (int j = 0; j < count; j++)
        //        {
        //            var consted = BSBmParams.QueryReadNewParameters(parametred, streamQueryParameters);
        //            SparqlQuery sparqlQuery = SparqlQueryParser.Parse(Store, consted);
        //            SparqlResultSet sparqlResultSet = sparqlQuery.Run();
        //            sparqlResultSet.ToJson();
        //        }
        //        Console.WriteLine("history count " + Store.history.Count);
        //        using (StreamWriter sr = new StreamWriter(@"..\..\output.txt", true))
        //        {
        //            sr.WriteLine("q"+i);
        //            sr.WriteLine(Store.Output());
        //        }
        //        //Store.TrainingMode = false;

        //        //RunTestParametred(Store, count: 1);
        //    }
        //}
        //public static void CacheMeasureAllWithConstants<T>(T Store) where T : CacheMeasure, IStore
        //{
        //    Store.TrainingMode = true;


        //    Console.WriteLine("bsbm with constants train");


        //    int i=8;
        //    string file = string.Format(@"..\..\examples\bsbm\queries\with constants\{0}.rq", i);
        //    var readAllText = File.ReadAllText(file);
        //    var sparqlQuery = SparqlQueryParser.Parse(Store, readAllText);
        //    sparqlQuery.Run().ToJson();

        //    Console.WriteLine("history count " + Store.history.Count);
        //    using (StreamWriter sr = new StreamWriter(@"..\..\output.txt", true))
        //    {  sr.WriteLine(i);
        //    sr.WriteLine(Store.Output());  }

        //    //for ideal cache
        //    //cacheMeasure.TrainingMode = false;
        //    //Console.WriteLine("bsbm with constants from history");
        //    //RunTestParametred(50, 1);

        //}
    
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
                        sparqlQuery = SparqlQueryParser.Parse(Store, readAllText);
                    }

                    totalparseMS[i] += GetTimeWthLast2Digits(timer);
                    var st1 = DateTime.Now;
                    //if (i == 0)
                    {
                        var sparqlResultSet = sparqlQuery.Run().ToJson();                        
                    }
                    totalrun[i] += (DateTime.Now - st1).Ticks / 10000L;
                    var totalMilliseconds = GetTimeWthLast2Digits(timer);

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

        public static void RunBerlinsWithConstants()
        {               
            double[] memoryUsage = new double[12];
            double[] totalparseMS = new double[12];
            double[] totalCreateLinqStack = new double[12];
            double[] totalrun = new double[12];
            Console.WriteLine("bsbm with constants");
                var timer = new Stopwatch();
          //  using (
           // var Store = new Store("../../../Databases/int based/");
            var Store = new RDFRamStore();
         
            //Store.ActivateCache();
            {
                Store.ReloadFrom(Config.Source_data_folder_path+"1M.ttl");
             //     Store.Start();
                SparqlQueryParser.Parse((IStore) Store, sq5);

                for (
                         int i = 0;
                i < 12; i++)
                {
                    string file = string.Format(@"examples\bsbm\queries\with constants\{0}.rq", i + 1);
                    var readAllText = File.ReadAllText(file);

                    GC.Collect();
                    timer.Restart();
                    var sparqlQuery = SparqlQueryParser.Parse((IStore) Store, readAllText);
                    timer.Stop();
                    totalparseMS[i] += GetTimeWthLast2Digits(timer);
                    timer.Restart();
                    var sparqlResultSet = sparqlQuery.Run();
                    timer.Stop();
                    totalCreateLinqStack[i] += GetTimeWthLast2Digits(timer);
                    timer.Restart();
                    sparqlResultSet.Results.ToArray();
                    timer.Stop();
                    totalrun[i] += GetTimeWthLast2Digits(timer);

                    memoryUsage[i] = GC.GetTotalMemory(false);
                    File.WriteAllText(Path.ChangeExtension(file, ".json"), sparqlResultSet.ToJson());

                    //results[i]=
                }
            }
            using (StreamWriter r = new StreamWriter(@"..\..\output.txt", true))
                {   
                    r.WriteLine("date time " + DateTime.Now);
                    r.WriteLine("memory usage (bytes)" + string.Join(", ", memoryUsage));
                    r.WriteLine("parse (ms)" + string.Join(", ", totalparseMS));
                    r.WriteLine("total parse " + totalparseMS.Sum());
                    r.WriteLine("create linq stack " + string.Join(", ", totalCreateLinqStack));
                    r.WriteLine("total create linq stack " + totalCreateLinqStack.Sum());
                    r.WriteLine("run " + string.Join(", ", totalrun));
                    r.WriteLine("parse + create linq stack + run " + string.Join(", ", totalrun.Select((d, i) => d+totalCreateLinqStack[i]+totalparseMS[i]).Select(d=>d.ToString().Replace(",","."))));
                    r.WriteLine("total run " + totalrun.Sum());
                    r.WriteLine("total " + 
                        (totalparseMS.Sum() + totalCreateLinqStack.Sum() + totalrun.Sum()));
                }


        }

        public static double GetTimeWthLast2Digits(Stopwatch timer)
        {
            return timer.ElapsedMilliseconds>10 ? timer.ElapsedMilliseconds :
                ((double)((int)(timer.ElapsedTicks / 100)))/100;
        }


     




        public static void RunTestParametred(int count = 100)
        {

            var Store = new Store("../../../Databases/int based/");
            Store.ReloadFrom(Config.Source_data_folder_path + "1.ttl");
            SparqlQueryParser.Parse((IStore) Store, sq5);
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
                        var sparqlResults = SparqlQueryParser.Parse((IStore) Store, q).Run();

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
                            SparqlQueryParser.Parse((IStore) Store, q).Run().Results.Count());
                    }
                }
            }
        }

        private static string sq51 = @" PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX bsbm: <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/>
PREFIX dataFromProducer1: <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/instances/dataFromProducer1/> 

SELECT ?product
WHERE { 
	dataFromProducer1:Product12 bsbm:productFeature ?prodFeature .
	?product bsbm:productFeature ?prodFeature .
    FILTER (dataFromProducer1:Product12 != ?product)	
?product rdfs:label ?productLabel .
	dataFromProducer1:Product12 bsbm:productPropertyNumeric1 ?origProperty1 .
	?product bsbm:productPropertyNumeric1 ?simProperty1 .
#	FILTER (?simProperty1 < (?origProperty1 + 120) && ?simProperty1 > (?origProperty1 - 120))
     }
";

        private static string sq52 = @" PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX bsbm: <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/>
PREFIX dataFromProducer1: <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/instances/dataFromProducer1/> 

SELECT DISTINCT ?product
WHERE { 
	dataFromProducer1:Product12 bsbm:productFeature ?prodFeature .
	?product bsbm:productFeature ?prodFeature .
    FILTER (dataFromProducer1:Product12 != ?product)	
?product rdfs:label ?productLabel .
	dataFromProducer1:Product12 bsbm:productPropertyNumeric1 ?origProperty1 .
	?product bsbm:productPropertyNumeric1 ?simProperty1 .
	FILTER (?simProperty1 < (?origProperty1 + 120) && ?simProperty1 > (?origProperty1 - 120))
     }
";
        private static string sq = @"SELECT  ?prodFeature
WHERE { 
 <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/instances/dataFromProducer1/Product12>  <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/productFeature> ?prodFeature .
	?product <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/productFeature> ?prodFeature .
}
";

        private static string sq5 = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX bsbm: <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/>
PREFIX dataFromProducer1: <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/instances/dataFromProducer1/> 

SELECT DISTINCT ?product ?productLabel
WHERE { 
	dataFromProducer1:Product12 bsbm:productFeature ?prodFeature .
	?product bsbm:productFeature ?prodFeature .
    FILTER (dataFromProducer1:Product12 != ?product)	
	?product rdfs:label ?productLabel .
	dataFromProducer1:Product12 bsbm:productPropertyNumeric1 ?origProperty1 .
	?product bsbm:productPropertyNumeric1 ?simProperty1 .
	FILTER (?simProperty1 < (?origProperty1 + 120) && ?simProperty1 > (?origProperty1 - 120))
	dataFromProducer1:Product12 bsbm:productPropertyNumeric2 ?origProperty2 .
	?product bsbm:productPropertyNumeric2 ?simProperty2 .
	FILTER (?simProperty2 < (?origProperty2 + 170) && ?simProperty2 > (?origProperty2 - 170))
}
ORDER BY ?productLabel
LIMIT 5
";
        private static readonly string _queryString = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX bsbm: <http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/>

SELECT ?product ?label
WHERE {
    ?product rdf:type bsbm:Product .
	?product rdfs:label ?label .
	FILTER regex(?label, ""^s"")}";

       public static void OneBerlinParametrized(IStore store, int i, int count)
        {

          SparqlQueryParser.Parse(store, sq5);      
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
                    var sparqlQuery = SparqlQueryParser.Parse(store, consted);
                    timer.Stop();

                    double time1 = SparqlTesting.GetTimeWthLast2Digits(timer);
                    averageParse += (double) ((int) (100*time1/count))/100;

                    timer.Restart();
                    var sparqlResultSet = sparqlQuery.Run();
                    timer.Stop();

                    double time2 = SparqlTesting.GetTimeWthLast2Digits(timer);
                    averageCLS += (double) ((int) (100*time2/count))/100;

                    timer.Restart();
                    sparqlResultSet.Results.ToArray();
                    timer.Stop();

                    double time3 = SparqlTesting.GetTimeWthLast2Digits(timer);
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

           SparqlQueryParser.Parse(store, sq5);
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
                   var sparqlQuery = SparqlQueryParser.Parse(store, q);
                   timer.Stop();

                   double time1 = SparqlTesting.GetTimeWthLast2Digits(timer);
                   averageParse += (double)((int)(100 * time1 / count)) / 100;

                   timer.Restart();
                   var sparqlResultSet = sparqlQuery.Run();
                   timer.Stop();

                   double time2 = SparqlTesting.GetTimeWthLast2Digits(timer);
                   averageCLS += (double)((int)(100 * time2 / count)) / 100;

                   timer.Restart();
                   sparqlResultSet.Results.ToArray();
                   timer.Stop();

                   double time3 = SparqlTesting.GetTimeWthLast2Digits(timer);
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