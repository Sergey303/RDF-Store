using System;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Node
{
    using System.Xml;

    public class SparqlUnDefinedNode : ObjectVariants
    {
        public override ObjectVariantEnum Variant
        {
            get { return ObjectVariantEnum.Undef; }
        }

        public override object WritableValue
        {
            get { throw new NotImplementedException(); }
        }

        public override object Content
        {
            get { throw new NotImplementedException(); }
        }

        public static ObjectVariants Undef =new SparqlUnDefinedNode();
        public override string ToString()
        {
            return "UNDEF";
        }

        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            throw new NotImplementedException();
        }

        public override void ReadXml(XmlReader reader)
        {
            
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("undef");
            writer.WriteAttributeString("type", this.GetType().ToString());
            writer.WriteEndElement();
        }
    }
}