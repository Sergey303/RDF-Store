using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    public class SparqlMinusGraphPattern : ISparqlGraphPattern
    {
        private ISparqlGraphPattern sparqlGraphPattern;
        private readonly RdfQuery11Translator q;

        public SparqlMinusGraphPattern(ISparqlGraphPattern sparqlGraphPattern, RdfQuery11Translator q)
        {
            // TODO: Complete member initialization
            this.sparqlGraphPattern = sparqlGraphPattern;
            this.q = q;
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            var minusResults = this.sparqlGraphPattern.Run(Enumerable.Repeat(new SparqlResult(this.q), 1))
             .Select(result => result.Clone()).ToArray();
            var before = variableBindings.Select(result => result.Clone()).ToArray();
            var after = before.Where(result =>
                minusResults.All(minusResult =>
                    minusResult.TestAll((minusVar, minusValue) =>
                    {
                        var value = result[minusVar];
                        return value == null || !Equals(minusValue, value);
                    })));
            return after;
        }

        public SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.Minus;} }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this.sparqlGraphPattern = (ISparqlGraphPattern)Query.SparqlQuery.CreateByTypeAttribute(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("minus");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.sparqlGraphPattern.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
