using System;
using RDFCommon.OVns.general;

namespace RDFCommon.OVns
{
    public class OV_Blank : OV_iri
    {
        

        public OV_Blank(string fullId)        :base("Http://iis.nsk.su/.well-known/genid/" +fullId)
        {
        
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Blank; }
        }

        public override object WritableValue
        {
            get { return UriString; }
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

            return uriString == ((OV_Blank)obj).uriString;

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            var hashCode = uriString.GetHashCode();
            return unchecked((11 ^ hashCode) * (13 ^ Variant.GetHashCode()));
        }


        public override string ToString()
        {
            return uriString;
        }

      
        public override object Content
        {
            get { return uriString; }
        }

        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            //return new OV_Blank(changing(uriString));
            throw new NotImplementedException();
        }

        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_Blank)obj;
            return System.String.Compare(uriString, otherTyped.uriString, System.StringComparison.InvariantCulture);
        }
        
      
    }
}