using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.general
{
    public class OV_string : ObjectVariants, ILiteralNode, IStringLiteralNode
    {
        public OV_string()
        {
            
        }
        public string value;

        public OV_string(string value)
        {
            this.value = value;
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Str; }
        }

        public override object WritableValue
        {
            get { return value; }
        }


        // override object.Equals
        public override bool Equals(object obj)
        {
       
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var @equals = value == ((OV_string)obj).value;
            return @equals;

        }

        

        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_string(changing(value));
        }

        public string DataType { get { return SpecialTypesClass.String; } }
        public override string ToString()
        {
            return value;
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_string)obj;
            return System.String.Compare(value, otherTyped.value, System.StringComparison.InvariantCulture);
        }
    }
}