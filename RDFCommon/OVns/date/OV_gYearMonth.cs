using System;
using System.Collections.Generic;
using RDFCommon.Interfaces;
using RDFCommon.OVns.numeric;

namespace RDFCommon.OVns.date
{
    public class OV_gYearMonth : ObjectVariants, ILiteralNode  
    {
        public readonly KeyValuePair<int, short> value;

        public OV_gYearMonth(string s)
        {
            throw new NotImplementedException();
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Int; }
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

            return value.Equals(((OV_gYearMonth)obj).value);

        }


        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_float(changing(value));
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
            var otherTyped = (OV_gYearMonth)obj;
            var y = value.Key.CompareTo(otherTyped.value.Key);
            return y !=0 ? y : value.Value.CompareTo(otherTyped.value.Value);
        }
    }
}