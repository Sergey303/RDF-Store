using System.Linq;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples;
using SparqlQuery.SparqlClasses.Query.Result;
using SparqlQuery.SparqlClasses.SolutionModifier;

namespace SparqlQuery.SparqlClasses.Query
{
    using System.Xml;

    public class SparqlConstructQuery :SparqlQuery
    {
      
        private SparqlGraphPattern constract;

        public SparqlConstructQuery(RdfQuery11Translator q) : base(q)
        {
           
        }
 
        internal void Create(SparqlSolutionModifier sparqlSolutionModifier)
        {
            this.SparqlSolutionModifier = sparqlSolutionModifier;
        }

        internal void Create(SparqlGraphPattern sparqlTriples)
        {
            this.sparqlWhere = sparqlTriples;
        }


        internal void Create(SparqlGraphPattern sparqlTriples, ISparqlGraphPattern sparqlWhere, SparqlSolutionModifier sparqlSolutionModifier)
        {
            this.constract = sparqlTriples;
            this.sparqlWhere = sparqlWhere;
            this.SparqlSolutionModifier = sparqlSolutionModifier;
        }

        public override SparqlResultSet Run()
        {
           base.Run();
            this.ResultSet.GraphResult = this.q.Store.CreateTempGraph();
            foreach (var result in this.ResultSet.Results)
                foreach (var st in this.constract.Cast<SparqlTriple>())
                    st.Substitution(result, (s, p, o) => this.ResultSet.GraphResult.Add(s, p, o));
            return this.ResultSet;
        }

        // public override SparqlQueryTypeEnum QueryType
        // {
        // get { return SparqlQueryTypeEnum.Construct; }
        // }
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("constract");

            base.WriteXml(writer);

            writer.WriteEndElement();
        }
    }

}
