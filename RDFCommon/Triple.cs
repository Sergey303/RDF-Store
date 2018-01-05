
using System;
using RDFCommon.Hashes;
using RDFCommon.OVns;

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
    public class TripleIntOV : Triple<int, int, ObjectVariants>{
        public TripleIntOV(int s, int p, ObjectVariants o) : base(s, p, o)
        {
        }
        public object[] ToWritable()
        {
            return new object[] { (object)Subject, (object)Predicate, (object)Object.ToWritable() };
        }

    }
    public class TripleOV :Triple<ObjectVariants, ObjectVariants,ObjectVariants>{
        public TripleOV(ObjectVariants s, ObjectVariants p, ObjectVariants o) : base(s, p, o)
        {
        }
    }
         public class TripleStrOV :Triple<string, string,ObjectVariants>{
             public TripleStrOV(string s, string p, ObjectVariants o)
                 : base(s, p, o)
        {
        }
    }     
    public class Pair<T1, T2> : IComparable
        where T1 : IComparable
        where T2 : IComparable
    {
        readonly T1 p;
        readonly T2 ov;
        public  Pair(T1 predicate, T2 ov) { this.p = predicate; this.ov = ov; }
        public int CompareTo(object another)
        {
            var ano = ( Pair<T1, T2>)another;
            int cmp = this.GetHashCode().CompareTo(ano.GetHashCode());
            if (cmp == 0)
            {
                cmp = this.p.CompareTo(ano.p);
            }
            if (cmp == 0 )
            {
                cmp = this.ov.CompareTo(ano.ov);
            }
            return cmp;
        }
        public override int GetHashCode()
        {
            return (int) (ov.ToString() + p).GetHashModifiedBernstein();
        }

    }
    public class POPairInt : Pair<int,ObjectVariants> {
        public POPairInt(int predicate, ObjectVariants ov)
            : base(predicate, ov)
        {
        }
    }
    public class SPPairInt : Pair<int, int>
    {
        public SPPairInt(int predicate, int ov)
            : base(predicate, ov)
        {
        }
    }
    public class POPairStr : Pair<string, ObjectVariants>
    {
        public POPairStr(string predicate, ObjectVariants ov)
            : base(predicate, ov)
        {
        }
    }
    public class SP_Pair : IComparable
    {
        readonly int s;
        readonly int p;
        public SP_Pair(int subject, int predicate) { this.s = subject; this.p = predicate; }
        //int S { get; set; }
        //int P { get; set; }
        public int CompareTo(object another)
        {
            SP_Pair ano = (SP_Pair)another;
            int cmp = this.GetHashCode().CompareTo(ano.GetHashCode());
            if (cmp == 0)
            {
                cmp = this.s.CompareTo(ano.s);
            }
            if (cmp == 0)
            {
                cmp = this.p.CompareTo(ano.p);
            }
            return cmp;
        }
        public override int GetHashCode()
        {
            //return s.GetHashCode() ^ p.GetHashCode();
            return (3001 ^ s.GetHashCode()) * (1409 ^ p.GetHashCode());
        }
    }
    public class SPO_Troyka : IComparable
    {
        readonly int s;
        readonly int p;
        readonly ObjectVariants ov;
        public SPO_Troyka(int subject, int predicate, ObjectVariants ov) { this.s = subject; this.p = predicate; this.ov = ov; }
        public int CompareTo(object another)
        {
            SPO_Troyka ano = (SPO_Troyka)another;
            int cmp = this.GetHashCode().CompareTo(ano.GetHashCode());
            if (cmp == 0)
            {
                cmp = this.s.CompareTo(ano.s);
            }
            if (cmp == 0)
            {
                cmp = this.p.CompareTo(ano.p);
            }
            if (cmp == 0)
            {
                cmp = this.ov.CompareTo(ano.ov);
            }
            return cmp;
        }
        public override int GetHashCode()
        {
            return (2 ^ s.GetHashCode()) * (3 ^ p.GetHashCode()) * (7 ^ ov.GetHashCode());
        }
    }
    public class PO_Pair : IComparable
    {
        readonly int p;
        readonly ObjectVariants ov;
        public PO_Pair(int predicate, ObjectVariants ov) { this.p = predicate; this.ov = ov; }
        public int CompareTo(object another)
        {
            PO_Pair ano = (PO_Pair)another;
            int cmp = this.GetHashCode().CompareTo(ano.GetHashCode());
            if (cmp == 0)
            {
                cmp = this.p.CompareTo(ano.p);
            }
            if (cmp == 0)
            {
                cmp = this.ov.CompareTo(ano.ov);
            }
            return cmp;
        }

        public override int GetHashCode()
        {
            return (int) (ov.ToString() + p).GetHashModifiedBernstein();
        }
    }

    public struct TripleOVStruct
    {
        public ObjectVariants Subject, Predicate, Object;

        public TripleOVStruct(ObjectVariants s, ObjectVariants p, ObjectVariants o)
        {
            Subject = s;
            Predicate = p;
            Object = o;
        }
    }
    public struct QuadOVStruct
    {
        public ObjectVariants Subject, Predicate, Object, Graph;
        public QuadOVStruct(ObjectVariants s, ObjectVariants p, ObjectVariants o, ObjectVariants g)
        {
            Subject = s;
            Predicate = p;
            Object = o;
              Graph = g;
        }
    }
}