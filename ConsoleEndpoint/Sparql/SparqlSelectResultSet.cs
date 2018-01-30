namespace SparqlQuery.SparqlClasses.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using global::SparqlQuery.SparqlClasses.Query.Result;

    using RDFCommon;

    public class SparqlSelectResultSet : SparqlResultSet
    {
        private readonly HashSet<string> Variables = new HashSet<string>();

        private readonly List<string> @select;

        private readonly bool isReduced;

        private readonly bool isDistinct;

        public SparqlSelectResultSet(
            HashSet<string> sparqlWhereVariables,
            bool isReduced=false,
            bool isDistinct=false,
            List<string> @select=null,
            Prologue qProlog = null)
            : base(qProlog)
        {
            this.@select = @select;
            this.isReduced = isReduced;
            this.isDistinct = isDistinct;
            this.Variables = sparqlWhereVariables;
        }
        

        private IEnumerable<TRow> Transform<T, TRow>(
            Func<IEnumerable<T>, TRow> rowAggregate,
            Func<string, object, T> oneVarSelect) =>
            this.Results.Select(result => rowAggregate(this.Selection.Select(var => oneVarSelect(var, result[var]))));

        public List<string> Selection { get; set; }

        public override string ToCommaSeparatedValues()
        {
            var headVars = string.Join(",", this.Variables);

            string OneVarSelect(string var, object value) => value is object[] ov ? ov[1].ToString() : value.ToString();

            string RowAggregate(IEnumerable<string> row) => string.Join(",", row);

            return headVars + Environment.NewLine + string.Join(
                       Environment.NewLine,
                       this.Transform(RowAggregate, OneVarSelect));
        }

        public void Run()
        {
            this.Selection = this.@select ?? this.Variables.ToList(); // is all if null

            if (this.isDistinct)
            {
                Results = Distinct();
            }

            if (this.isReduced)
            {
                Results = Reduce();
            }
        }

        private IEnumerable<SparqlResult> Reduce()
        {
            return null;
            //todo reduce
        }

        private IEnumerable<SparqlResult> Distinct()
        {
            var history = this.Selection.Select(var => (var, new HashSet<object>())).ToList();
            foreach (var res in Results)
            {
                bool any = false;
                foreach (var tuple in history)
                {
                    var(var, exists) = tuple;
                    if (exists.Contains(res[var]))
                    {
                        any = true;
                       break;
                    }
                }

                if (!any)
                {
                    foreach (var tuple in history)
                    {
                    var(var, exists) = tuple;
                        exists.Add(res[var]);
                    }

                    yield return res;
                }
            }
        }
    }
}