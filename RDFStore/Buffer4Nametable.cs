using System;
using System.Collections.Generic;
using RDFCommon.Hashes;
using RDFCommon.Interfaces;
using RDFCommon.long_dictionary;

namespace RDFStore
{
    public class Buffer4Nametable : IGetDictionaryLong<string, int>
    {
        // унаследовано  public readonly DictionaryLong<string, int> NotSaved;
        public DictionaryLong<string, int> Inserted;
        private List<KeyValueHash<string,int, ulong>>  allreadyInserted;
        private DictionaryLong<string, int> _notSaved = null;
        public DictionaryLong<string, int> NotSaved { get { return _notSaved; } }
        public Buffer4Nametable(Func<string, ulong> hashFunc, ulong notSavedcapacity, ulong insertCapacity, float factor = 30F) 
        {
            _notSaved = new DictionaryLong<string, int>(hashFunc, notSavedcapacity, factor);
            Inserted = new DictionaryLong<string, int>(hashFunc, insertCapacity, factor);
            allreadyInserted= new List<KeyValueHash<string, int, ulong>>();
        }
        public int this[string key]
        {
            get
            {
                var kvhash = TryGetValue(new KeyValueHash<string, int, ulong>(key, key.GetHashSpooky()) { Value = -1});
                if (kvhash.HasValue) return kvhash.Value;
                return -1;
            }
            //set { popular.Add(key, value); }
        }

        private LongStream<string> keys;
        public IGetLongStream<string> Keys { get { return keys ?? ( keys= LongStream<string>.Concat(NotSaved.Keys, Inserted.Keys)); } }

        public IEnumerable<KeyValueHash<string, int, ulong>> FilterExisting(IEnumerable<KeyValueHash<string,int, ulong>> portion)
        {
            allreadyInserted.Clear();
            foreach (var s in portion)
            {
                var kvhash = TryGetValue(s);
                if (kvhash.HasValue) continue;
                kvhash = Inserted.TryGetValue(kvhash);
                if (kvhash.HasValue)
                {
                    allreadyInserted.Add(kvhash);
                    continue;
                }
                yield return kvhash;
            }
            Inserted.Clear();
            foreach (var keyValuePair in allreadyInserted)
                Inserted.Add(keyValuePair.Key, keyValuePair.Value, keyValuePair.Hash);
        }

        public void OnSaved()
        {
            NotSaved.Clear();
            //TODO Рейтинг популярности
            //popular.Clear();
            //foreach (var strCod in popularCopy)
            //    popular.Add(strCod.Key, strCod.Value);
            //Console.WriteLine("poplar codes count "+popularCopy.Count);
            //popularCopy.Clear();
        }

        public  bool ContainsKey(string key)
        {
            return NotSaved.ContainsKey(key) || Inserted.ContainsKey(key);
        }

        public  KeyValueHash<string, int, ulong> TryGetValue(KeyValueHash<string, int, ulong> key)
        {
            var keyValueHash = Inserted.TryGetValue(key);
            if(keyValueHash.HasValue)
                return keyValueHash;
            return NotSaved.TryGetValue(keyValueHash);
        }

        public  bool TryGetValue(string key, out int value)
        {
            return NotSaved.TryGetValue(key, out value) || Inserted.TryGetValue(key, out value);
        }
        
        private LongStream<int> values;

        public IGetLongStream<int> Values
        {
            get { return values ?? (values = LongStream<int>.Concat(NotSaved.Values, Inserted.Values)); }
        }


        public  void FreeMemory()
        {
            NotSaved.FreeMemory();
            //popularCopy = new List<KeyValuePair<string, int>>();
            //popular.FreeMemory();
        }

        public ulong Count {
            get
            {
                //Console.WriteLine(NotSaved.Keys.Count);
                //Console.WriteLine("INSERTED " +inserted.Keys.Count);
                return NotSaved.Keys.Count + Inserted.Keys.Count;
            } }

        public void Clear()
        {
            NotSaved.Clear();
            Inserted.Clear();
            if(values!=null)
                values.Clear();
            if(keys!=null)
                keys.Clear();
        }
    }
}