using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;
    using System.Xml.Schema;

    public class SparqlUpdateDeleteData : ISparqlUpdate
    {
        private SparqlQuadsPattern sparqlQuadsPattern;

        public SparqlUpdateDeleteData(SparqlQuadsPattern sparqlQuadsPattern)
        {
            // TODO: Complete member initialization
            this.sparqlQuadsPattern = sparqlQuadsPattern;
        }

        public void Run(IStore store)
        {
           
            foreach (var triple in this.sparqlQuadsPattern
                .Where(pattern => pattern.PatternType == SparqlGraphPatternType.SparqlTriple)
                .Cast<SparqlTriple>())
            {
                store.Delete(triple.Subject, triple.Predicate, triple.Object);
                
            }

            foreach (var sparqlGraphGraph in
                this.sparqlQuadsPattern.Where(pattern => Equals(pattern.PatternType, SparqlGraphPatternType.Graph))
                    .Cast<SparqlGraphGraph>())                                                                     
            {
               // if (sparqlGraphGraph.Name == null)
                    // store.NamedGraphs.DeleteFromAll(
                      // sparqlGraphGraph.GetTriples().Select(t => new Triple<ObjectVariants, ObjectVariants>(t.Subject, t.Predicate, t.Object)));
                // store.NamedGraphs.Delete(sparqlGraphGraph.Name, sparqlGraphGraph.GetTriples().Select(t => new Triple<ObjectVariants, ObjectVariants>(t.Subject, t.Predicate, t.Object)));
            }
        }
        public  void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("deleteData");
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
            sparqlQuadsPattern= (SparqlQuadsPattern) Query.SparqlQuery.CreateByTypeAttribute(reader);
        }

    }
}
