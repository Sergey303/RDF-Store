using RDFCommon.OVns;

namespace RDFCommon
{
    public class TripleStrOV :Triple<string, string,ObjectVariants>{
        public TripleStrOV(string s, string p, ObjectVariants o)
            : base(s, p, o)
        {
        }
    }
}