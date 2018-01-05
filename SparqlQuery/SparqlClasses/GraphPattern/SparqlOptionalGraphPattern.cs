using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    public class SparqlOptionalGraphPattern : ISparqlGraphPattern
    {
        private ISparqlGraphPattern sparqlGraphPattern;

        public SparqlOptionalGraphPattern(ISparqlGraphPattern sparqlGraphPattern)
        {
            // TODO: Complete member initialization
            this.sparqlGraphPattern = sparqlGraphPattern;
        }
        
      
        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            
            foreach (var variableBinding in variableBindings)
            {
                var any = false;
                foreach (var resultVariableBinding in this.sparqlGraphPattern.Run(Enumerable.Repeat(variableBinding, 1)))
                {
                    yield return resultVariableBinding;
                    any = true;
                }

                if (any) continue;

                yield return variableBinding;
            }
        }

        public SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.Optional;} }
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
            writer.WriteStartElement("optional");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.sparqlGraphPattern.WriteXml(writer);
            writer.WriteEndElement();

        }
    }
}
