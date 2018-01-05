using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.numeric
{
    public class OV_float : ObjectVariants, ILiteralNode, INumLiteral
    {
        public readonly float value;

        public OV_float(float value)
        {
            this.value = value;
        }

        public OV_float(string s): this(float.Parse(s))
        {
            

        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Float; }
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

            return value == ((OV_float)obj).value;

        }



        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_float(changing(value));
        }

        public string DataType { get { return SpecialTypesClass.Float; } }
        public override string ToString()
        {
            return value.ToString();
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_float)obj;
            return value.CompareTo(otherTyped.value);
        }
    }
}