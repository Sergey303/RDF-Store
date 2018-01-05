using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.numeric
{
    public class OV_negativeInteger : ObjectVariants, ILiteralNode, INumLiteral
    {
        public readonly int value;

        public OV_negativeInteger(int value)
        {
            this.value = value;
        }

        public OV_negativeInteger(string s) : this(int.Parse(s))
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

            return value == ((OV_negativeInteger)obj).value;

        }

        


        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_negativeInteger(changing(value));
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
            var otherTyped = (OV_negativeInteger)obj;
            return value.CompareTo(otherTyped.value);
        }
    }
}