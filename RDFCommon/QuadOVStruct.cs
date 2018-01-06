using RDFCommon.OVns;

namespace RDFCommon
{
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