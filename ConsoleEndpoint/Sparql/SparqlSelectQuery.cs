namespace ConsoleEndpoint.Sparql
{
    using System.Collections.Generic;
    using System.Linq;

    using ConsoleEndpoint.Interfaces;

    public class SparqlSelectQuery : SparqlQuery
    {

          readonly  SparqlSelectResultSet SelectResultSet;

        public SparqlSelectQuery(
            ISparqlGraphPattern[] sparqlWhere, 
            bool isReduced = false, 
            bool isDistinct = false, 
            List<string> @select = null, 
            Prologue prolog = null)
            : base(sparqlWhere)
        {
            this.SelectResultSet = new SparqlSelectResultSet(
                new HashSet<string>(sparqlWhere.SelectMany(pattern => pattern.Variables)),
                isReduced,
                isDistinct,
                @select,
                prolog);
        }
      
        public SparqlSelectResultSet Run(IStore store) => this.Run<SparqlSelectResultSet>(store);

        public override T Run<T>(IStore store)
        {
            this.SelectResultSet.Results = this.sparqlWhere.Aggregate(
                Enumerable.Repeat<SparqlResult>(new SparqlResult(), 1),
                (current, pattern) => pattern.Run(current, store));
            this.SelectResultSet.Run();
            return this.SelectResultSet as T;
        }
    }
}
