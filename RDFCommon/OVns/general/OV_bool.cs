using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.general
{
    public class OV_bool : ObjectVariants, ILiteralNode
    {
        public readonly bool value;

        public OV_bool(bool value)
        {
            this.value = value;
        }

        public OV_bool(string s) : this( bool.Parse(s))
        {
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Bool; }
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

            return value == ((OV_bool)obj).value;

        }


        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_bool(changing(value));
        }

        public string DataType { get { return SpecialTypesClass.Bool; } }
        public static OV_bool True =new OV_bool(true);
        public static OV_bool False =new OV_bool(false);

        public override string ToString()
        {
            return value.ToString();
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_bool)obj;           
            return value.CompareTo(otherTyped.value);
        }
    }
}