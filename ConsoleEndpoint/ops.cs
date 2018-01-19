namespace ConsoleEndpoint
{
    using System;

    public struct ops : IComparable<ops>, IComparable
    {
        private (OV o, int p, int s) triple;

        public ops((int s, int p, OV o) triple)
        {
            this.triple = (triple.o, triple.p, triple.s);
        }

        // В этом представлении сравнение идет по приоритету o, p, s
        public int CompareTo(ops other)
        {
            return this.triple.CompareTo(other.triple);
        }

        public int CompareTo(object obj)
        {
            return ((IComparable)this.triple).CompareTo(obj);
        }
    }
}