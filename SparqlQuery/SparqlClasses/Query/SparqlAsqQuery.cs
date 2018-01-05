using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.Query.Result;
using SparqlQuery.SparqlClasses.SolutionModifier;

namespace SparqlQuery.SparqlClasses.Query
{
    using System.Xml;

    public class SparqlAsqQuery : SparqlQuery
    {

        public SparqlAsqQuery(RdfQuery11Translator q) : base(q)
        {
         
        }

        internal void Create(SparqlGraphPattern sparqlWhere, SparqlSolutionModifier sparqlSolutionModifier)
        {
            this.sparqlWhere = sparqlWhere;
            this.SparqlSolutionModifier = sparqlSolutionModifier;
        }

        public override SparqlResultSet Run()
        {
            base.Run();
            return this.ResultSet;

        }
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ask");

            base.WriteXml(writer);

            writer.WriteEndElement();
        }

        // public override SparqlQueryTypeEnum QueryType
        // {
        // get { return SparqlQueryTypeEnum.Ask; }
        // }
    }
}
