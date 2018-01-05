using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.general
{
    public class OV_typed : ObjectVariants, ILiteralNode
    {
        public OV_typed()
        {
            
        }
        public string value; public string turi;

        public OV_typed(string value, string turi)
        {
            this.value = value;
            this.turi = turi;
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Typed; }
        }

        public override object WritableValue
        {
            get
            {
                return new object[] { value, turi };
            }
        }

  

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = ((OV_typed)obj);
            return value == other.value && turi.Equals(other.turi);
        }



        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_typed(changing(value), turi);
        }

        public string DataType { get { return turi; } }
        public override string ToString()
        {
            return "\"" + value + "\"^^<"+DataType+">";
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_typed)obj;
            var cmpBase = String.Compare(DataType, otherTyped.DataType, StringComparison.InvariantCulture);
            //if (obj is OV_langstring) //если совпали варианты, то и типы идентичны.
            if (cmpBase != 0) return cmpBase;
            return System.String.Compare(value, otherTyped.value, System.StringComparison.InvariantCulture);

        }
    }
}