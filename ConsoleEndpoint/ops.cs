namespace ConsoleEndpoint
{
    using System;

    using ConsoleEndpoint.Comparers;

    public struct ops : IComparable<ops>, IComparable
    {
        private object[] o;

        private int p;

        private int s;

        public ops((int s, int p, object[] o) t)
        {
            (this.s, this.p, this.o) = t;
        }

        public int CompareTo(ops other)
        {
            var oComparison = ObjectVariantComparer.Default.Compare(this.o, other.o); 
            if (oComparison != 0) return oComparison;
            var pComparison = this.p.CompareTo(other.p);
            if (pComparison != 0) return pComparison;
            return this.s.CompareTo(other.s);
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo((ops)obj);
        }
    }
}