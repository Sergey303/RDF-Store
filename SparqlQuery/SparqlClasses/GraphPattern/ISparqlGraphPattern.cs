using System.Collections.Generic;
using System.Xml.Serialization;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    public interface ISparqlGraphPattern   : IXmlSerializable
    {
        IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings);
        SparqlGraphPatternType PatternType { get; }
    }
}