using System;
using RDFCommon.Interfaces;

namespace RDFCommon.long_dictionary
{
    public class DictionaryLong<TKey, TValue> : Coding<TKey>, IGetDictionaryLong<TKey, TValue>
    {
        private readonly LongStream<TValue> values;


        public virtual IGetLongStream<TValue> Values
        {
            get { return values; }
        }

        public DictionaryLong(Func<TKey, ulong> hashFunc, ulong capacity=100000, float factor = 3F) : base(hashFunc, capacity, factor)
        {
            values=new LongStream<TValue>(capacity);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            ulong codeForSearch;
            if (base.TryGetValue(key, out codeForSearch))
            {
                value = Values[codeForSearch];
                return true;
            }
            value = default(TValue);
            return false;
        }
        public virtual KeyValueHash<TKey, TValue, ulong> TryGetValue(KeyValueHash<TKey, TValue, ulong> key)
        {
            var coded = new KeyValueHash<TKey, ulong, ulong>(key.Key, key.Hash);

            coded = base.TryGetValue(coded);
            if (coded.HasValue)
            {
                key.Value = Values[coded.Value];
                key.HasValue = true;
                return key;
            }
            return key;
        }

        public virtual ulong Add(TKey key, TValue value, ulong hash = 0)
        {
            ulong codeForSearch = base.Add(key, hash);
            if (codeForSearch >= values.Length)
                values.Add(value);
            return codeForSearch;
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                ulong codeForSearch;
                return TryGetValue(key, out codeForSearch) ? values[codeForSearch] : default(TValue);
            }
            set { Add(key, value, 0); }
        }



        public override void Clear()
        {
            values.Clear();
            base.Clear();
        }

        public override void FreeMemory()
        {
            values.FreeMemory();
            base.FreeMemory();
        }

        public ulong Count { get { return Keys.Count; } }
    }
}
