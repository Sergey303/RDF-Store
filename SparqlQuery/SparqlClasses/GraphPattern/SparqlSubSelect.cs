using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using SparqlQuery.SparqlClasses.Query.Result;
using SparqlQuery.SparqlClasses.SolutionModifier;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    public class SparqlSubSelect : Query.SparqlQuery, ISparqlGraphPattern
    {   

        public SparqlSubSelect(ISparqlGraphPattern sparqlWhere, SparqlSolutionModifier sparqlSolutionModifier, ISparqlGraphPattern sparqlValueDataBlock, RdfQuery11Translator q) 
            :base(q)
        {
            // TODO: Complete member initialization
            this.sparqlWhere = sparqlWhere;
            this.SparqlSolutionModifier = sparqlSolutionModifier;
            this.valueDataBlock = sparqlValueDataBlock;

            // this.sparqlValueDataBlock = sparqlValueDataBlock;
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            foreach (var sparqlResult in variableBindings)
            {
                this.Seed = Enumerable.Repeat<SparqlResult>(sparqlResult, 1);
                var sparqlResultSet = this.Run();
                foreach (var result in sparqlResultSet.Results)
                    yield return result;
            }
            
        }

        public SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.SubSelect;}}

        public XmlSchema GetSchema()
        {
            return null;
        }

        public override void ReadXml(XmlReader reader)
        {

          base.ReadXml(reader);
        }

        public override void WriteXml(XmlWriter writer)
        {

            writer.WriteStartElement("subSelect");
            base.WriteXml(writer);
            writer.WriteEndElement();

        }
    }
}
