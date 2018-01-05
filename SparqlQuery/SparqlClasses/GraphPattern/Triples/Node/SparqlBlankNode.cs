using RDFCommon;
using RDFCommon.Interfaces;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Node
{
    using System.Xml;

    public class SparqlBlankNode : VariableNode, IBlankNode
    {


        public SparqlBlankNode(string varName, int count):base(varName, count)
        {
            
        }

        public SparqlBlankNode(int count)    :base("blank var"+count, count)
        {
         
        }


        public string Name
        {
            get { return this.VariableName; }
        }

        public override void ReadXml(XmlReader reader)
        {
            this.VariableName = reader.GetAttribute("name");
            this.index =int.Parse( reader.GetAttribute("n"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("blank");
            writer.WriteAttributeString("type", this.GetType().ToString());
            writer.WriteAttributeString("name", this.VariableName);
            writer.WriteAttributeString("n", this.index.ToString());
            writer.WriteEndElement();
        }
    }
}