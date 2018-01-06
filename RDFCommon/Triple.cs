
using System;
using RDFCommon.Hashes;

namespace RDFCommon
{
    public class Triple<Ts, Tp, To> : IComparable
        where Ts : IComparable
        where Tp : IComparable
        where To:IComparable
    {
        private readonly Ts subject;
        private readonly Tp predicate;
        private readonly To o;


        public Ts Subject
        {
            get { return subject; }
        }

        public Tp Predicate
        {
            get { return predicate; }
        }

        public To Object
        {
            get { return o; }
        }

        public Triple(Ts s, Tp p, To o)
            {
                this.subject = s;
                this.predicate = p;
                this.o = o;
            }

            public int CompareTo(object another)
            {
                var  ano = (Triple<Ts, Tp, To> )another;
                int cmp = this.GetHashCode().CompareTo(ano.GetHashCode());
                if (cmp == 0) cmp = Subject.CompareTo(ano.Subject);
                if (cmp == 0) cmp = Predicate.CompareTo(ano.Predicate);
                if (cmp == 0) cmp = Object.CompareTo(ano.Object);
                return cmp;
            }
            public override int GetHashCode()
            {
                return (int) (o.ToString() + subject + predicate).GetHashModifiedBernstein();
            }
        }
}