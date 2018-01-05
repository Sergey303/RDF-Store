using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.general
{
    public class OV_langstring : ObjectVariants, ILanguageLiteral 
    {
        public readonly string value; public readonly string lang;

        public OV_langstring(string value, string lang)
        {
            this.value = value;
            this.lang = lang;
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.LanguagedString; }
        }

        public override object WritableValue
        {
            get { return new object[] { value, lang }; }
        }
     

        // override object.Equals
        public bool Equals(OV_langstring other)
        {
            return lang == other.lang && value == other.value;  
        }

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
            var other = ((OV_langstring)obj);
            return lang == other.lang && value == other.value;  
        }

        


        public string Lang { get { return lang; } }
        public override object Content { get { return value; } }
        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            return new OV_langstring(changing(value), lang);
        }

        public string DataType { get { return SpecialTypesClass.LangString; } }
        public override string ToString()
        {
            return value.ToString();
        }

        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_langstring)obj;
            var cmpBase = String.Compare(lang, otherTyped.lang, StringComparison.InvariantCulture);
            //if (obj is OV_langstring) //если совпали варианты, то и типы идентичны.
            if (cmpBase != 0) return cmpBase;
            return System.String.Compare(value, otherTyped.DataType, System.StringComparison.InvariantCulture);

        }
    }

}