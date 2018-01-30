using System.Collections.Generic;

using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    using ConsoleEndpoint.Interface;

    public interface ISparqlGraphPattern 
    {
        IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings, IStore store);
       // SparqlGraphPatternType PatternType { get; }
        HashSet<string> Variables { get; }
    }
}