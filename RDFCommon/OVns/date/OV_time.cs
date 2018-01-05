using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.date
{
    public class OV_time : ObjectVariants, ILiteralNode
    {
        public readonly DateTimeOffset value;

        public OV_time(DateTimeOffset value)
        {
            this.value = value;
        }

        public OV_time(string s)
        {                      
            if (DateTimeOffset.TryParse(s, out value))
                return;
            DateTime date;
            DateTime.TryParse(s, out date);
            value = new DateTimeOffset(date);
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Time; }
        }

        public override object WritableValue
        {
            get { return value.Ticks; }
        }

        public override object Content
        {
            get { return value; }
        }

        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_time(changing(value));
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

            return value == ((OV_time)obj).value;

        }

        


        public override string ToString()
        {
            return value.ToString();
        }
        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_time)obj;
            return value.CompareTo(otherTyped.value);
        }

        public string DataType { get { return SpecialTypesClass.DayTimeDuration; } }
    }
}