using RDFCommon.OVns;

namespace RDFCommon
{
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
}