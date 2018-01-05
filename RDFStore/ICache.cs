using System;
using System.Collections.Generic;

namespace RDFStore
{
    class Cache<TKey, TValue>
    {
        Dictionary<TKey, TValue> cacheDictionary = new Dictionary<TKey, TValue>();
        Func<TKey, TValue> getValue;

        public Cache(Func<TKey, TValue> getValue)
        {
            this.getValue = getValue;
        }

        public TValue Get(TKey key)
        {
            TValue value;
            if (cacheDictionary.TryGetValue(key, out value)) return value;
            if (cacheDictionary.Keys.Count > 1000 * 1000 * 1000)
                cacheDictionary.Clear();
            cacheDictionary.Add(key,value= getValue(key));
            return value;
        }

        public bool Contains(TKey key)
        {
            return cacheDictionary.ContainsKey(key);
        }
        public void Add(TKey k, TValue v)
        {
            if (cacheDictionary.ContainsKey(k)) return;
            cacheDictionary.Add(k, v);
        }
    }
    class Cache<TKey1, TKey2, TValue>
    {
        Dictionary<TKey1, Dictionary<TKey2, TValue>> cacheDictionary = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
        Func<TKey1, TKey2, TValue> getValue;

        public Cache(Func<TKey1, TKey2, TValue> getValue)
        {
            this.getValue = getValue;
        }

        public TValue Get(TKey1 k1, TKey2 k2)
        {
            TValue value;
            Dictionary<TKey2, TValue> d2;
            if (!cacheDictionary.TryGetValue(k1, out d2))
                cacheDictionary.Add(k1, new Dictionary<TKey2, TValue>() {{k2, value = getValue(k1, k2)}});
            else
            {
                if (!d2.TryGetValue(k2, out value))
                    d2.Add(k2, value = getValue(k1, k2));
                return value;
            }
            return value;
        }

        //public void Add(TKey1 k1, TKey2 k2, TValue v)
        //{
        //    var key = new KeyValuePair<TKey1, TKey2>(k1, k2);
        //    if (cacheDictionary.ContainsKey(key)) return;
        //    cacheDictionary.Add(key, v);
        //}
        //public bool Contains(TKey1 k1, TKey2 k2)
        //{
        //    KeyValuePair<TKey1, TKey2> key = new KeyValuePair<TKey1, TKey2>(k1, k2);
        //    return cacheDictionary.ContainsKey(key);
        //}
    }
    class Cache<TKey1, TKey2, TKey3, TValue> 
    {
        Dictionary<Tuple<TKey1, TKey2, TKey3>, TValue> cacheDictionary = new Dictionary<Tuple<TKey1, TKey2, TKey3>, TValue>();
        Func<TKey1, TKey2, TKey3, TValue> getValue;

        public Cache(Func<TKey1, TKey2, TKey3, TValue> getValue)
        {
            this.getValue = getValue;
        }

        public TValue Get(TKey1 k1, TKey2 k2, TKey3 k3)
        {
            TValue value;
            Tuple<TKey1, TKey2, TKey3> key = new Tuple<TKey1, TKey2, TKey3>(k1, k2, k3);
            if (cacheDictionary.TryGetValue(key, out value)) return value;
            if (cacheDictionary.Keys.Count > 1000 * 1000 * 1000)
                cacheDictionary.Clear();
            cacheDictionary.Add(key, value= getValue(k1, k2, k3));
            return value;
        }

        public void Add(TKey1 k1, TKey2 k2, TKey3 k3, TValue v)
        {
            Tuple<TKey1, TKey2, TKey3> key = new Tuple<TKey1, TKey2, TKey3>(k1, k2, k3);
            if (cacheDictionary.ContainsKey(key)) return;
            cacheDictionary.Add(key, v);
        }
        public bool Contains(TKey1 k1, TKey2 k2, TKey3 k3)
        {
            Tuple<TKey1, TKey2, TKey3> key = new Tuple<TKey1, TKey2, TKey3>(k1, k2, k3);
            return cacheDictionary.ContainsKey(key);
        }
    }
    class Cache<TKey1, TKey2, TKey3, TKey4, TValue>
    {
        Dictionary<Tuple<TKey1, TKey2, TKey3, TKey4>, TValue> cacheDictionary = new Dictionary<Tuple<TKey1, TKey2, TKey3, TKey4>, TValue>();
        Func<TKey1, TKey2, TKey3, TKey4, TValue> getValue;

        public Cache(Func<TKey1, TKey2, TKey3, TKey4, TValue> getValue)
        {
            this.getValue = getValue;
        }

        public TValue Get(TKey1 k1, TKey2 k2, TKey3 k3, TKey4 k4)
        {
            TValue value;
            Tuple<TKey1, TKey2, TKey3, TKey4> key = new Tuple<TKey1, TKey2, TKey3, TKey4>(k1, k2, k3, k4);
            if (cacheDictionary.TryGetValue(key, out value)) return value;
            if (cacheDictionary.Keys.Count > 1000 * 1000 * 1000)
                cacheDictionary.Clear();
            cacheDictionary.Add(key, value=getValue(k1, k2, k3, k4));
            return value;
        }

        public void Add(TKey1 k1, TKey2 k2, TKey3 k3, TKey4 k4, TValue v)
        {
            var key = new Tuple<TKey1, TKey2, TKey3, TKey4>(k1, k2, k3, k4);
            if (cacheDictionary.ContainsKey(key)) return;
            cacheDictionary.Add(key, v);

        }
        public bool Contains(TKey1 k1, TKey2 k2, TKey3 k3, TKey4 k4)
        {
            Tuple<TKey1, TKey2, TKey3, TKey4> key = new Tuple<TKey1, TKey2, TKey3, TKey4>(k1, k2, k3, k4);
            return cacheDictionary.ContainsKey(key);
        }
    }
}