using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleEndpoint.Interfaces
{
  public interface IStore4SparqlOptimise
    {
        uint GetCountOfSubject(int iri);
        uint GetCountOfPredicate(int iri);
        uint GetCountOfObject(object[] coded);
    }
}
