using System;
using RDFCommon.Interfaces;

namespace RDFCommon.long_dictionary
{
    public class Coding<TKey>
    {
        public virtual IGetLongStream<TKey> Keys
        {
            get { return keys; }
        }

        private readonly Func<TKey, ulong> hashFunc;
        private LongStream<ulong> redirectHashToCode;
        private readonly LongStream<TKey> keys;

        public Coding(Func<TKey, ulong> hashFunc, ulong capacity=100000, float factor=3F)
        {
            this.hashFunc = hashFunc;
        
          var  maxHash = Primes.GetNextPrime((ulong) (capacity * factor));
            
            redirectHashToCode = new LongStream<ulong>(maxHash);
            for (ulong i = 0; i < maxHash; i++)
                redirectHashToCode.Add(ulong.MaxValue);
            keys = new LongStream<TKey>(capacity);
        }


      
        
        public virtual ulong Add(TKey key, ulong hash = 0)
        {
            while (true)
            {
                var increment = Hash2Position(hash ==0 ? hashFunc(key) : hash);
                
                for (ulong index = increment, i = 0; i < redirectHashToCode.Count; i++)
                {
                    ulong code = redirectHashToCode[index];
                    if (code == ulong.MaxValue)
                    {
                        redirectHashToCode[index] = GetNewCode();
                        keys.Add(key);
                        return redirectHashToCode[index];
                    }
                    if (Equals(Keys[code], key))
                    {
                        return code;
                    }
                    ulong newindex = (index + increment)%redirectHashToCode.Count;
                    index = newindex == index ? index + 1 : newindex;
                }
                Grow();
            }
        }

        public virtual void Clear()
        {
            keys.Clear();
            redirectHashToCode.Position = 0;
            for (ulong i = 0L; i < redirectHashToCode.Length; i++)
                redirectHashToCode.Write(ulong.MaxValue);
        }

        public virtual bool ContainsKey(TKey key)
        {
            ulong c;
            return TryGetValue(key, out c);
        }

        public virtual bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }
        public virtual bool TryGetValue(TKey key, out ulong code)
        {
            var keyCodeHash = TryGetValue(new KeyValueHash<TKey, ulong, ulong>(key, hashFunc(key) ));
            if (keyCodeHash.HasValue)
            {
                code = keyCodeHash.Value;
                return true;
            }
            else
            {
                code = ulong.MaxValue;
                return false;
            }
        }

        
        public virtual KeyValueHash<TKey, ulong, ulong> TryGetValue(KeyValueHash<TKey,ulong, ulong> key)
        {
            if (key.HasValue) return key;
            
            var increment = Hash2Position(key.Hash);
           var position = increment;
            for (ulong i = 0;
                i < redirectHashToCode.Count;
                i++)
            {

               key.Value = redirectHashToCode[position];
                if (key.Value == ulong.MaxValue)
                {
                    return key;
                }
                if (Keys[key.Value].Equals(key.Key))
                {
                    key.HasValue = true;
                    return key;
                }
                ulong newindex = (position + increment) % redirectHashToCode.Count;
                position = newindex == position ? position + 1 : newindex;
            }
            return key;
        }

        private ulong GetNewCode()
        {
            return Keys.Count;
        }

        private void Grow()
        {
            ulong newLength= Primes.GetNextPrime((ulong)(redirectHashToCode.Length * 2)); ;
            redirectHashToCode = new LongStream<ulong>(newLength);
            var keysCopy = Keys.Copy();
            keys.Clear();
            for (ulong i = 0; i < keysCopy.Length; i++)
            {
                Add(keysCopy[i]);
            }
        }

        private ulong Hash2Position(ulong hash)
        {
            return hash % redirectHashToCode.Count;
        }


        public virtual void FreeMemory()
        {
            redirectHashToCode.FreeMemory();
            Keys.FreeMemory();
        }
    }
    public struct KeyValueHash<TKey, TValue, THash>
    {
        public TKey Key;
        public TValue Value;
        public THash Hash;
        public bool HasValue;
     //   public ulong Position;

        public KeyValueHash(TKey key, THash hash)
        {
       //     Position = ulong.MaxValue;
            Key = key;
            Hash = hash;
            Value = default(TValue);
            HasValue = false;
        }
    }
}
