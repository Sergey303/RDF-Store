namespace ConsoleEndpoint
{
    using System;

    public struct spo : IComparable<spo>, IComparable
    {
        public int s;

        public int p;

        public OV o;

        public spo((int s, int p, OV o) triple)
        {
            this.s =triple.s;
            this.p = triple.p;
            this.o = triple.o;
        }

        // В этом представлении сравнение идет по приоритету s, p, o
        public int CompareTo(spo other)
        {
            var sComparison = this.s.CompareTo(other.s);
            if (sComparison != 0) return sComparison;
            var pComparison = this.p.CompareTo(other.p);
            if (pComparison != 0) return pComparison;
            return this.o.CompareTo(other.o);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((spo)obj);
        }
    }
}