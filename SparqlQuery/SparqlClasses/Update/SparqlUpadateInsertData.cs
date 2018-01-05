using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;
    using System.Xml.Schema;

    public class SparqlUpdateInsertData : ISparqlUpdate
    {
        private SparqlQuadsPattern sparqlQuadsPattern;

        public SparqlUpdateInsertData(SparqlQuadsPattern sparqlQuadsPattern)
        {
            // TODO: Complete member initialization
            this.sparqlQuadsPattern = sparqlQuadsPattern;
        }

        public  void Run(IStore store)
        {
            //  throw new NotImplementedException();

            store.Add(sparqlQuadsPattern.Where(pattern => pattern.PatternType == SparqlGraphPatternType.SparqlTriple)
            .Cast<SparqlTriple>().Select(t => new TripleOV(t.Subject, t.Predicate, t.Object)));
            foreach (var sparqlGraph in
            sparqlQuadsPattern.Where(pattern => pattern.PatternType == SparqlGraphPatternType.Graph)
            .Cast<SparqlGraphGraph>()
            //.Where(graph => store.NamedGraphs.ContainsGraph(graph.Name))
            )
                store.NamedGraphs.Add(sparqlGraph.Name, sparqlGraph.GetTriples().Select(t => new TripleOV(t.Subject, t.Predicate, t.Object)));
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("insertData");
            writer.WriteAttributeString("type", this.GetType().ToString());
            sparqlQuadsPattern.WriteXml(writer);
           
            writer.WriteEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            sparqlQuadsPattern = (SparqlQuadsPattern)Query.SparqlQuery.CreateByTypeAttribute(reader);

        }
    }
}
