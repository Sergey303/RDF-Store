namespace ConsoleEndpoint.Sparql
{
    using System.Collections.Generic;
    using System.Linq;

    using ConsoleEndpoint.Interfaces;

    public class Union : List<ISparqlGraphPattern>, ISparqlGraphPattern
    {
       

        public Union(params ISparqlGraphPattern[] sparqlGraphPattern)
        {
            // TODO: Complete member initialization
            this.AddRange(sparqlGraphPattern);
        }


        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings, IStore store)
        {
            return from result in variableBindings
                   from graphPattern in this
                   from sparqlResult in graphPattern.Run(Enumerable.Repeat(result, 1), store)
                   select sparqlResult;
        }

        public IEnumerable<string> Variables => this.SelectMany(pattern => pattern.Variables);
    }
}
