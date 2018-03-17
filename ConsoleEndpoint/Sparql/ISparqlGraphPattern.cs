namespace ConsoleEndpoint.Sparql
{
    using System.Collections.Generic;

    using ConsoleEndpoint.Interfaces;

    public interface ISparqlGraphPattern 
    {
        IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings, IStore store);
       // SparqlGraphPatternType PatternType { get; }
        IEnumerable<string> Variables { get; }
    }
}