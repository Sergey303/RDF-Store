namespace ConsoleEndpoint.Sparql
{
    using System.Collections.Generic;

    public abstract class SparqlResultSet
    {
        public IEnumerable<SparqlResult> Results;

        private readonly Prologue prologue;

        public SparqlResultSet(Prologue prologue)
        {
            this.prologue = prologue;
        }

        public abstract string ToCommaSeparatedValues();
    }
}