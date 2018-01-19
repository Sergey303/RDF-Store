using System;
using System.Linq;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.Query.Result;
using SparqlQuery.SparqlClasses.SolutionModifier;

namespace SparqlQuery.SparqlClasses.Query
{
    using System.Xml;

    [Serializable]
    public class SparqlSelectQuery : SparqlQuery
    {
        /// 4 serialization only
        public SparqlSelectQuery()
        {
            
        }
    
        public SparqlSelectQuery(RdfQuery11Translator q) : base(q)
        {
            this.ResultSet.ResultType = ResultType.Select;
          
        }

        internal void Create( SparqlGraphPattern sparqlWhere, SparqlSolutionModifier sparqlSolutionModifier)
        {
            this.sparqlWhere = sparqlWhere;
            this.SparqlSolutionModifier = sparqlSolutionModifier;

            // this.sparqlSolutionModifier.IsDistinct=sparqlSelect.IsDistinct;
            // if (this.sparqlSolutionModifier.IsDistinct)
            // sparqlSelect.IsDistinct = false;
        }

        public override SparqlResultSet Run()
        {
            this.ResultSet.Variables = this.q.Variables;
            this.ResultSet.Results=Enumerable.Repeat(new SparqlResult(this.q), 1);
            this.ResultSet.Results = this.sparqlWhere.Run(this.ResultSet.Results);
            
            if (this.SparqlSolutionModifier != null ) this.ResultSet.Results = this.SparqlSolutionModifier.Run(this.ResultSet.Results, this.ResultSet);
          
            return this.ResultSet;
        }
  
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("selectQuery");

            base.WriteXml(writer);

            writer.WriteEndElement();
        }
        // public override SparqlQueryTypeEnum QueryType
        // {
        // get { return SparqlQueryTypeEnum.Select; }
        // }
    }
}
