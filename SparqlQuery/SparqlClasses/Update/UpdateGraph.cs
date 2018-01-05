using System;
using SparqlQuery.SparqlClasses.GraphPattern;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    public class UpdateGraph :IXmlSerializable
    {
        public SparqlGrpahRefTypeEnum SparqlGrpahRefTypeEnum;
        public string Name;
                                                   
        public UpdateGraph(string uriNode)
        {
            // TODO: Complete member initialization
            this.Name = uriNode;
            this.SparqlGrpahRefTypeEnum = SparqlGrpahRefTypeEnum.Setted;
        }

        public UpdateGraph(SparqlGrpahRefTypeEnum sparqlGrpahRefTypeEnum1)
        {
            // TODO: Complete member initialization
            this.SparqlGrpahRefTypeEnum = sparqlGrpahRefTypeEnum1;
        }

        public XmlSchema GetSchema()
        {
            return null;
            
        }

        public void ReadXml(XmlReader reader)
        {
            Enum.TryParse( reader.GetAttribute("GrpahRefType"), true, out SparqlGrpahRefTypeEnum);

            Name = reader.ReadString();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("graph");
            writer.WriteAttributeString("type", this.GetType().ToString());

            writer.WriteAttributeString("GrpahRefType", this.SparqlGrpahRefTypeEnum.ToString());
            writer.WriteString(this.Name);
            
            writer.WriteEndElement();
        }
    }
}