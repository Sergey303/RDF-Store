using RDFCommon.OVns;

namespace RDFCommon
{
    public class TripleOV :Triple<ObjectVariants, ObjectVariants,ObjectVariants>{
        public TripleOV(ObjectVariants s, ObjectVariants p, ObjectVariants o) : base(s, p, o)
        {
        }
    }
}