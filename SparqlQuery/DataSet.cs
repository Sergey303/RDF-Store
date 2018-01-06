using System.Collections.Generic;
using RDFCommon.OVns;

namespace SparqlQuery
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    public class DataSet : HashSet<ObjectVariants>, IXmlSerializable
    {
        public DataSet(IEnumerable<ObjectVariants> gs)
            :base(gs)
        {
        }

        public DataSet()                 
        {
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {

            while (reader.IsStartElement())
            {
                this.Add((ObjectVariants)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader));
            }
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("graphs");
            writer.WriteAttributeString("type", this.GetType().ToString());
            foreach (var graph in this) graph.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}