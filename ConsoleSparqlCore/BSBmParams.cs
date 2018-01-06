using System;
using System.IO;
using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;

namespace ConsoleSparql
{
    public  class BSBmParams
    {
        private readonly IStore store;

        public BSBmParams(IStore store)
        {
            this.store = store;
            _products = Enumerable.ToArray<ObjectVariants>(this.store.GetSubjects(store.NodeGenerator.GetUri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                store.NodeGenerator.GetUri("http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/Product")));
            _productCount = _products.Count();

            _offers = Enumerable.ToArray<ObjectVariants>(this.store.GetSubjects(
                store.NodeGenerator.GetUri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                store.NodeGenerator.GetUri("http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/Offer")));
            _offersCount = _offers.Count();

            _review = Enumerable.ToArray<ObjectVariants>(this.store.GetSubjects(
                store.NodeGenerator.GetUri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                store.NodeGenerator.GetUri("http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/Review")));
            _reviewCount = _review.Count();
            _productFeature = Enumerable.ToArray<ObjectVariants>(this.store.GetSubjects(
                store.NodeGenerator.GetUri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                store.NodeGenerator.GetUri("http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/ProductFeature")));
            _productFeatureCount = _productFeature.Count();
            _productType = Enumerable.ToArray<ObjectVariants>(this.store.GetSubjects(
                store.NodeGenerator.GetUri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                store.NodeGenerator.GetUri("http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/vocabulary/ProductType")));
            _productTypeCount = _productType.Count();
            random = new Random();
            words = File.ReadAllLines(@"C:\bsbm\titlewords.txt");
        }

        private readonly ObjectVariants[] _products;
            //).ToArray();

        private readonly int _productCount;
        private readonly int _offersCount;
        private readonly int _reviewCount;
        private readonly int _productFeatureCount;
        private readonly int _productTypeCount;
        private readonly Random random;
        private readonly ObjectVariants[] _offers;
        private readonly ObjectVariants[] _review;
        private readonly ObjectVariants[] _productFeature;
        private readonly ObjectVariants[] _productType;
        private readonly string[] words;


        private void QueryWriteParameters(string parameteredQuery, StreamWriter output)
        {
          
            if (parameteredQuery.Contains("%ProductType%"))
                output.WriteLine(_productType[random.Next(0, _productTypeCount)]);
            if (parameteredQuery.Contains("%ProductFeature1%"))
                output.WriteLine(_productFeature[random.Next(0, _productFeatureCount)]);
            if (parameteredQuery.Contains("%ProductFeature2%"))
                output.WriteLine(_productFeature[random.Next(0, _productFeatureCount)]);
            if (parameteredQuery.Contains("%ProductFeature3%"))
                output.WriteLine(_productFeature[random.Next(0, _productFeatureCount)]);
            if (parameteredQuery.Contains("%x%")) output.WriteLine(random.Next(1, 500).ToString());
            if (parameteredQuery.Contains("%y%")) output.WriteLine(random.Next(1, 500).ToString());
            if (parameteredQuery.Contains("%ProductXYZ%"))
                output.WriteLine(_products.ElementAt(random.Next(0, _productCount)));  //"<http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/instances/dataFromProducer{0}/Product{1}>",productProducer, product);
            if (parameteredQuery.Contains("%word1%")) output.WriteLine(words[random.Next(0, words.Length)]);
            if (parameteredQuery.Contains("%currentDate%"))
                output.WriteLine("\"" + DateTime.Today.AddYears(-7) + "\"^^<http://www.w3.org/2001/XMLSchema#dateTime>");
            if (parameteredQuery.Contains("%ReviewXYZ%"))
                output.WriteLine(_review[random.Next(0, _reviewCount)]);//"<http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/instances/dataFromRatingSite{0}/Review{1}>",review/10000 + 1, review);
            if (parameteredQuery.Contains("%OfferXYZ%"))
                output.WriteLine(_offers[random.Next(0, _offersCount)]); ////"<http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/instances/dataFromVendor{0}/Offer{1}>", vendor, offer);
        }

        public static string QueryReadParameters(string parameteredQuery, StreamReader input)
        {
            if (parameteredQuery.Contains("%ProductType%"))
                parameteredQuery = parameteredQuery.Replace("%ProductType%", Read(input.ReadLine()));
            if (parameteredQuery.Contains("%ProductFeature1%"))
                parameteredQuery = parameteredQuery.Replace("%ProductFeature1%", Read(input.ReadLine() ));
            if (parameteredQuery.Contains("%ProductFeature2%"))
                parameteredQuery = parameteredQuery.Replace("%ProductFeature2%", Read(input.ReadLine() ));
            if (parameteredQuery.Contains("%ProductFeature3%"))
                parameteredQuery = parameteredQuery.Replace("%ProductFeature3%", Read(input.ReadLine()));
            if (parameteredQuery.Contains("%x%"))
                parameteredQuery = parameteredQuery.Replace("%x%", input.ReadLine());
            if (parameteredQuery.Contains("%y%"))
                parameteredQuery = parameteredQuery.Replace("%y%", input.ReadLine());
            if (parameteredQuery.Contains("%ProductXYZ%"))
                parameteredQuery = parameteredQuery.Replace("%ProductXYZ%", Read(input.ReadLine() ));
            if (parameteredQuery.Contains("%word1%"))
                parameteredQuery = parameteredQuery.Replace("%word1%", input.ReadLine());
            if (parameteredQuery.Contains("%currentDate%"))
                parameteredQuery = parameteredQuery.Replace("%currentDate%", input.ReadLine());
            if (parameteredQuery.Contains("%ReviewXYZ%"))
                parameteredQuery = parameteredQuery.Replace("%ReviewXYZ%", Read(input.ReadLine()));
            if (parameteredQuery.Contains("%OfferXYZ%"))
                parameteredQuery = parameteredQuery.Replace("%OfferXYZ%", Read(input.ReadLine()));
            return parameteredQuery;
        }
        public static string QueryReadNewParameters(string parameteredQuery, StreamReader input)
        {
            if (parameteredQuery.Contains("%ProductType%"))
                parameteredQuery = parameteredQuery.Replace("%ProductType%", "<" + input.ReadLine() + ">");
            if (parameteredQuery.Contains("%ProductFeature1%"))
                parameteredQuery = parameteredQuery.Replace("%ProductFeature1%", "<" + input.ReadLine() + ">");
            if (parameteredQuery.Contains("%ProductFeature2%"))
                parameteredQuery = parameteredQuery.Replace("%ProductFeature2%", "<" + input.ReadLine() + ">");
            if (parameteredQuery.Contains("%ProductFeature3%"))
                parameteredQuery = parameteredQuery.Replace("%ProductFeature3%", "<" + input.ReadLine() + ">");
            if (parameteredQuery.Contains("%x%"))
                parameteredQuery = parameteredQuery.Replace("%x%", input.ReadLine());
            if (parameteredQuery.Contains("%y%"))
                parameteredQuery = parameteredQuery.Replace("%y%", input.ReadLine());
            if (parameteredQuery.Contains("%ProductXYZ%"))
                parameteredQuery = parameteredQuery.Replace("%ProductXYZ%", "<" + input.ReadLine() + ">");
            if (parameteredQuery.Contains("%word1%"))
                parameteredQuery = parameteredQuery.Replace("%word1%", input.ReadLine());
            if (parameteredQuery.Contains("%currentDate%"))
                parameteredQuery = parameteredQuery.Replace("%currentDate%", input.ReadLine());
            if (parameteredQuery.Contains("%ReviewXYZ%"))
                parameteredQuery = parameteredQuery.Replace("%ReviewXYZ%", "<" + input.ReadLine() + ">");
            if (parameteredQuery.Contains("%OfferXYZ%"))
                parameteredQuery = parameteredQuery.Replace("%OfferXYZ%", "<" + input.ReadLine() + ">");
            return parameteredQuery;
        }

        private static string Read(string readLine)
        {
            if (readLine.StartsWith("http://"))
                return "<" + readLine + ">";
            var splitPrefixed = Prologue.SplitPrefixed(readLine);
            if (splitPrefixed.prefix == "bsbm-inst:")
            {
                string read = "<" + readLine.Replace("bsbm-inst:", "http://www4.wiwiss.fu-berlin.de/bizer/bsbm/v01/instances/") + ">";
                return read;
            }
            return readLine;
        }

        public void CreateParameters(int query, int count, int millions)
        {
            var paramvaluesFilePath = string.Format(@"..\..\examples\bsbm\queries\parameters\param values for{0}m {1} query.txt", millions, query);
            var paramvaluesFilePath2 = string.Format(@"..\..\examples\bsbm\queries\parameters\{0}.rq", query);
            using (StreamWriter streamParameters = new StreamWriter(paramvaluesFilePath, true))
            using (StreamReader streamQuery = new StreamReader(paramvaluesFilePath2))
            {
                string q = streamQuery.ReadToEnd();
                for (int j = 0; j < count; j++)
                {
                    QueryWriteParameters(q, streamParameters);
                }
            }
        }
    }
}