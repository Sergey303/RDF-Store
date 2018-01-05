using System;
using RDFCommon.Interfaces;

namespace RDFCommon.OVns.general
{
    public class OV_iri : ObjectVariants, IIriNode, IBlankNode
    {
        private readonly string uriString;

        public OV_iri(string fullId)
        {
            uriString = fullId;
        }

        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Iri; }
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

            if (obj == null)
            {
                return false;
            }
            var iri = obj as OV_iri;
            if(iri != null)
            return uriString == iri.uriString;
            var iriint = obj as OV_iriint;
            if(iriint != null)
                return uriString == iriint.UriString;
        return false;
        }

      

        public string UriString
        {
            get { return uriString; }
        }

        public string Name { get { return uriString; } }
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
            return new OV_iri(changing(uriString));
        }

        public override int CompareTo(object obj)
        {
            int baseComp = base.CompareTo(obj);
            if (baseComp != 0) return baseComp;
            var otherTyped = (OV_iri)obj;
            return System.String.Compare(uriString, otherTyped.uriString, System.StringComparison.InvariantCulture);
        }
        
      
    }
}