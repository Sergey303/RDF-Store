using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.date
{
    public class OV_dateTimeStamp : ObjectVariants, ILiteralNode
    {
        public readonly DateTimeOffset value;

        public OV_dateTimeStamp(DateTimeOffset value)
        {
            this.value = value;
        }

        public OV_dateTimeStamp(string s):this(DateTimeOffset.Parse(s)){
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.DateTimeZone; }
        }

        public override object WritableValue
        {
            get { return value.ToFileTime(); }
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

            return value == ((OV_dateTimeStamp)obj).value;

        }

        



        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_dateTimeStamp(changing(value));
        }

        public string DataType { get { return SpecialTypesClass.DayTimeDuration; } }
        public override string ToString()
        {
            return value.ToString();
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_dateTimeStamp)obj;
            return value.CompareTo(otherTyped.value);
        }
    }
}