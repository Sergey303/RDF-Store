using System;
using System.Collections.Generic;
using System.Linq;
using Polar.CellIndexes;
using Polar.Cells;
using Polar.DB;

namespace RDFStore.CascadingIndex
{
    public class IndexCascadingDynamic<Tkey> : IIndexCommon where Tkey : IComparable
    {
        public IndexCascadingImmutable<Tkey> index_arr;
        Dictionary<Tuple<int, Tkey>, List<object>> dictionary;
        //public TableView Table { get; set; }
        //public Func<object, int> Key1Producer { get; set; }
        //public Func<object, Tkey> Key2Producer { get; set; }
        //public Func<Tkey, int> Half2Producer { get; set; }
        public IndexCascadingDynamic(string path, IBearingTableImmutable table,
            Func<object, int> key1Producer, Func<object, Tkey> key2Producer, Func<Tkey, int> half2Producer)
        {
            index_arr = new IndexCascadingImmutable<Tkey>(path,
                table,
                key1Producer,
                key2Producer,
                half2Producer);
        }
        public IEnumerable<object> GetRecordsAll()
        {
            //TODO: Надо еще обработать словарь
            return index_arr.GetAll();
        }
        private Diapason GetDiapasonByKey1(int key1)
        {
            //TODO: Надо еще обработать словарь
            return index_arr.GetDiapasonByKey1(key1);
        }
        public IEnumerable<object> GetRecordsWithKeys(int key1, Tkey key2)
        {
            //TODO: Надо проверить словарь
            return index_arr.GetAllByKeys(key1, key2);
        }
        public IEnumerable<object> GetRecordsWithKey1(int key1)
        {
            //TODO: Надо еще обработать словарь
            var diap = index_arr.GetDiapasonByKey1(key1);
            return index_arr.GetAllInDiap(diap);
        }
        public IEnumerable<object> GetRecordsWithKey2(Tkey key2)
        {
            //TODO: Надо еще обработать словарь
            var keys1 = index_arr.GetKey1All().ToArray();
            return keys1.SelectMany(key1 => index_arr.GetAllByKeys(key1, key2));
        }
        public void Build()
        {
            index_arr.Build();
            dictionary = new Dictionary<Tuple<int, Tkey>, List<object>>();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void FillPortion(IEnumerable<TableRow> tableRows)
        {
            index_arr.FillPortion(tableRows);
        }

        public void FillFinish()
        {
            index_arr.FillFinish();
        }

        public void FillInit()
        {
            index_arr.FillInit();
        }

        public void CreateDiscaleDictionary() { index_arr.CreateDiscaleDictionary(); }
        public void Warmup() { }
        public void ActivateCache() { index_arr.ActivateCache(); }

        public void OnAppendElement(PaEntry entry) { throw new NotImplementedException(); }

        public void DropIndex()
        {
            index_arr.DropIndex();
            if(dictionary!=null)
            dictionary.Clear();
        }
    }
}
