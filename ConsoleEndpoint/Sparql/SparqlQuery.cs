namespace ConsoleEndpoint.Sparql
{
    using System.Collections.Generic;

    using ConsoleEndpoint.Interfaces;

    public abstract class SparqlQuery 
    {
        protected IEnumerable<ISparqlGraphPattern> sparqlWhere;

       // public SparqlSolutionModifier SparqlSolutionModifier;

      
        public SparqlQuery(IEnumerable<ISparqlGraphPattern> where)
        {
            this.sparqlWhere = where;
        }

        public abstract T Run<T>(IStore store) where T: SparqlResultSet;
    }
}
