using RDFCommon;
using RDFCommon.Interfaces;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;

    public class SparqlUpdateCreate : SparqlUpdateSilent
    {
        public string Graph;
        public override void RunUnSilent(IStore store)
        {
            store.NamedGraphs.CreateGraph(this.Graph);          
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            Graph = reader.ReadContentAsString();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("create");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
         writer.WriteString(this.Graph);
            writer.WriteEndElement();
        }
    }
}
