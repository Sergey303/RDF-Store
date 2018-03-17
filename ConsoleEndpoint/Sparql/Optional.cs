using System.Collections.Generic;

namespace ConsoleEndpoint.Sparql
{
    using System.Linq;

    using ConsoleEndpoint.Interfaces;

    class Optional  : List<ISparqlGraphPattern>, ISparqlGraphPattern
    {
        public Optional(params ISparqlGraphPattern[] subTemplates)
        {
            AddRange(subTemplates);
        }
        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings, IStore store)
        {
            foreach (var variableBinding in variableBindings)
            {
                var any = false;
                foreach (var resultVariableBinding in this.SelectMany(pattern => pattern.Run(Enumerable.Repeat(variableBinding, 1), store)))
                {
                    yield return resultVariableBinding;
                    any = true;
                }

                if (any)
                {
                    continue;
                }

                yield return variableBinding;
            }
        }

        public IEnumerable<string> Variables => this.SelectMany(pattern => pattern.Variables);
    }
}
