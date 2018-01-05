using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.numeric
{
    public class OV_decimal : ObjectVariants, ILiteralNode, INumLiteral
    {
        public readonly decimal value;

      
        public OV_decimal(decimal value)
        {
            this.value = value;
        }
        public OV_decimal(string s)
            : this(decimal.Parse(s))
    {}
        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Decimal; }
        }

        public override object WritableValue
        {
            get { return Convert.ToDouble(value); }
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
      
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return value == ((OV_decimal)obj).value;

        }

        


        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_decimal(changing(value));
        }

        public string DataType { get { return SpecialTypesClass.Decimal; } }
        public override string ToString()
        {
            return value.ToString();
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_decimal)obj;
            return value.CompareTo(otherTyped.value);
        }
    }
}