using RDFCommon.OVns;

namespace RDFCommon
{
    public class TripleIntOV : Triple<int, int, ObjectVariants>{
        public TripleIntOV(int s, int p, ObjectVariants o) : base(s, p, o)
        {
        }
        public object[] ToWritable()
        {
            return new object[] { (object)Subject, (object)Predicate, (object)Object.ToWritable() };
        }

    }
}