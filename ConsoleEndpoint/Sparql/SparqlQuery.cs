using ConsoleEndpoint.Interface;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.Query
{
    using System.Collections.Generic;

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
