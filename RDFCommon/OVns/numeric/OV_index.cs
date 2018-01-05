using System;

namespace RDFCommon.OVns.numeric
{
    public class OV_index : ObjectVariants
    {
        public readonly int value;

        public OV_index(int value)
        {
            this.value = value;
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Index; }
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

            return value == ((OV_index)obj).value;

        }

        
        

        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_index(changing(value));
        }

        public string DataType { get { return SpecialTypesClass.Integer; } }
        public override string ToString()
        {
            return value.ToString();
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_index)obj;
            return value.CompareTo(otherTyped.value);
        }
    }
}