using System;

namespace RDFCommon.long_dictionary
{
    /// <summary>
    /// Узел для массива. В нем k - индекс ключа в массиве ключей, f2 - значение хеш-функции 2 на этом ключе.  
    /// </summary>
    internal struct KeyNode
    {
        public int f2, k;
        internal const int EmptyIndex = Int32.MinValue; // "пустой" индекс
        //public KeyNode() { f2 = KeyNode.EmptyIndex; k = -1; }
        public static KeyNode Empty() { return new KeyNode(); } 
        public static int LookForIndex(int f1, int f2, KeyNode[] arr)
        {
            int ind = (f1 & (~(1 << 31))) % arr.Length;
            KeyNode nd;
            while ((nd = arr[ind]).k != -1)
            {
            }
            return ind;
        }
    }
}