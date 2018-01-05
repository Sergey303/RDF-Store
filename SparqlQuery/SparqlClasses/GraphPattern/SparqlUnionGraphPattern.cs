using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    public class SparqlUnionGraphPattern : SparqlQuadsPattern
    {
       

        public SparqlUnionGraphPattern(ISparqlGraphPattern sparqlGraphPattern)
        {
            // TODO: Complete member initialization
            this.Add(  sparqlGraphPattern);
        }
       

        public override IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            return this.SelectMany(graphPattern => graphPattern.Run(variableBindings));
        }

        public override SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.Union;} }
        public override XmlSchema GetSchema()
        {
            return null;
        }

        public override void ReadXml(XmlReader reader)
        {
            
            base.ReadXml(reader);

            }

        public override void WriteXml(XmlWriter writer)
        {

            writer.WriteStartElement("union");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
