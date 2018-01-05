using System;

namespace RDFCommon.long_dictionary
{
    public struct KeyOffsetPair<Tkey> where Tkey : IComparable
    {
        public Tkey Key;
        public long Offset;

        public KeyOffsetPair(object row)
        {
            object[] cells = (object[]) row;
            Key = (Tkey) cells[0];
            Offset = (long) cells[1];
        }

        public object ToObject()
        {
            return new object[] {Key, Offset};
        }
    }
}