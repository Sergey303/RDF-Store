using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern.Triples;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    public class SparqlGraphGraph :  ISparqlGraphPattern 
    {
        private SparqlGraphPattern sparqlTriples;
        public ObjectVariants Name;

        public SparqlGraphGraph(ObjectVariants sparqlNode)
        {
            // TODO: Complete member initialization
            this.Name = sparqlNode;
        }
      
        internal void AddTriples(SparqlGraphPattern sparqlTriples)
        {
            this.sparqlTriples = sparqlTriples;
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            return this.sparqlTriples.Run(variableBindings);
        }

        public SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.Graph;} }

        
        public IEnumerable<SparqlTriple> GetTriples()
        {
            return this.sparqlTriples.Where(pattern => Equals(pattern.PatternType, SparqlGraphPatternType.SparqlTriple))
                .Cast<SparqlTriple>();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = (ObjectVariants) Query.SparqlQuery.CreateByTypeAttribute(reader);
            sparqlTriples =  (SparqlGraphPattern) Query.SparqlQuery.CreateByTypeAttribute(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
           writer.WriteStartElement("graph");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.Name.WriteXml(writer);
            this.sparqlTriples.WriteXml(writer);
            writer.WriteEndElement();
        }
    }

}
