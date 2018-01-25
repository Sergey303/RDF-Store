namespace ConsoleEndpoint
{
    using System;

    using ConsoleEndpoint.Comparers;

    public struct spo : IComparable<spo>, IComparable
    {
        public int s;

        public int p;

        public object[] o;

        public spo((int s, int p, object[] o) triple)
        {
            (this.s, this.p, this.o) = triple;
        }

        // В этом представлении сравнение идет по приоритету s, p, o
        public int CompareTo(spo other)
        {
            var sComparison = this.s.CompareTo(other.s);
            if (sComparison != 0) return sComparison;
            var pComparison = this.p.CompareTo(other.p);
            if (pComparison != 0) return pComparison;
            return ObjectVariantComparer.Default.Compare(this.o, other.o);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((spo)obj);
        }
    }
}