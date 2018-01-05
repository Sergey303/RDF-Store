using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.numeric
{
    public class OV_byte : ObjectVariants, ILiteralNode, INumLiteral
    {
        public readonly byte value;

        public OV_byte(byte value)
        {
            this.value = value;
        }

        public OV_byte(string s) : this(byte.Parse(s))
        {
            
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Double; }
        }

        public override object WritableValue
        {
            get { return value; }
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return value == ((OV_byte)obj).value;

        }



        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_double(changing(value));
        }

        public string DataType { get { return SpecialTypesClass.Double; } }
        public override string ToString()
        {
            return value.ToString();
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_byte)obj;
            return value.CompareTo(otherTyped.value);
        }
    }
}