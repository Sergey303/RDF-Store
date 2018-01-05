using System;
using System.Linq;

namespace RDFCommon.long_dictionary
{
    class BloomFilter<T>
    {
        private readonly Func<T, int>[] hashFunctions;
        public LongStream<byte> byts;
        private static readonly double Ln2 = Math.Log(2);
        private long addedElementsCountMax;
        public long bitsLength;
        private long addedElementsCount = 0;
        private int usedHashesCount;

        public BloomFilter(long maxBitsLength, long approximatelyAddedElementsCount, params Func<T, int>[] hashFunctions)

        {
            bitsLength = maxBitsLength;
            byts = new LongStream<byte>();
            this.hashFunctions = hashFunctions;
            usedHashesCount = (int)Math.Min(hashFunctions.Length, bitsLength * Ln2 / approximatelyAddedElementsCount);
        }

        public BloomFilter(byte[] bytes, long approximatelyAddedElementsCount, params Func<T, int>[] hashFunctions)
        {
            byts = new LongStream<byte>();

            byts.Write(bytes, 0, (uint)bytes.Length);
         //todo load   bitsLength
            this.hashFunctions = hashFunctions;
            usedHashesCount = (int)Math.Min(hashFunctions.Length, bitsLength * Ln2 / approximatelyAddedElementsCount);

        }
        public void Add(T newElement)
        {
            addedElementsCount++;
            if (addedElementsCount == addedElementsCountMax)
            {
                //TODO can't recompute if have no elemnts
            }
            foreach (var hashFunction in hashFunctions.Take(usedHashesCount))
            {
                byts.Position= (ulong)hashFunction(newElement)/8;
                var byte1= byts.Read();
                byte1 = (byte)( byte1 & (1 << hashFunction(newElement)%8));
                byts.Position= (ulong)hashFunction(newElement)/8;
                byts.Write(byte1);
            }
        }
        
        private void UnuseHashFunction()
        {
            //TODO
        }

        public bool Has(T existedElement)
        {
            return hashFunctions.Take(usedHashesCount)
                .Select(hashFunction =>
                {
                    byts.Position = (ulong)(hashFunction(existedElement) / 8);
                    var byte1 = byts.Read();
                    return (byte1 & (1 << hashFunction(existedElement)%8))== (1 << hashFunction(existedElement) % 8);
                })
                .All(b => b);
        }

      
       
    }
    
}
