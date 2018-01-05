using System;
using System.Collections.Generic;
using System.Text;
using RDFCommon.Interfaces;
using RDFCommon.long_dictionary;

namespace RDFStore
{
    class NameTableDictionaryRam: INametable, IGetDictionaryLong<string, int>
    {
        private Dictionary<string, int> code=new Dictionary<string, int>();
        private List<string> decode=new List<string>();
        public void Clear()
        {
            code.Clear();
            decode.Clear();
        }

        public int Capacity { get; set; }
        public int GetCode(string s)
        {
            return code[s];
        }

        public string GetString(int c)
        {
            return decode[c];
        }

        public int GetSetCode(string s)
        {
            int c;
            if (!code.TryGetValue(s, out c))
            {
                code.Add(s, c=decode.Count);
                decode.Add(s);
            }
            return c;
        }

        public void Expand(int length_estimation, IEnumerable<string> keyflow)
        {
            InsertPortion(keyflow);
        }

        public void Save()
        {
          
        }

        public int this[string key]
        {
            get { return code[key]; }
        }

        public IGetLongStream<string> Keys { get; }
        public IGetLongStream<int> Values { get; }
        public bool ContainsKey(string key)
        {
            return ContainsKey(key);
        }

        public KeyValueHash<string, int, ulong> TryGetValue(KeyValueHash<string, int, ulong> key)
        {
            int value;
            if (code.TryGetValue(key.Key, out value))
            {
                key.HasValue = true;
                key.Value = value;
            }
            else
            {
                key.HasValue = false;
            }
            return key;
        }

        public bool TryGetValue(string key, out int value)
        {
            return code.TryGetValue(key, out value);
        }

        public void FreeMemory()
        {
           
        }

        public ulong Count { get; }

        public IGetDictionaryLong<string, int> InsertPortion(IEnumerable<string> unsorted)
        {
            foreach (var s in unsorted)
            {
                GetSetCode(s);
            }
            return this;
        }
    }
}
