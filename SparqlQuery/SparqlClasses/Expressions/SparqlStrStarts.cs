using System;

namespace SparqlQuery.SparqlClasses.Expressions
{
    internal class SparqlStrStarts : SparqlExpression
    {


        public SparqlStrStarts(SparqlExpression str, SparqlExpression pattern)
        {
            switch (NullablePairExt.Get(str.Const, pattern.Const))
            {
                case NP.bothNull:
                    this.Operator = result => str.Operator(result).StartsWith(pattern.Operator(result));
                    this.TypedOperator =
                        result => str.TypedOperator(result).Change(o => o.StartsWith(pattern.Operator(result)));
                    this.AggregateLevel = SetAggregateLevel(str.AggregateLevel, pattern.AggregateLevel);
                    break;
                case NP.leftNull:
                    this.Operator = result => str.Operator(result).StartsWith(pattern.Const.Content);
                    this.TypedOperator = result => str.TypedOperator(result).Change(o => o.StartsWith(pattern.Const.Content));
                    this.AggregateLevel = str.AggregateLevel;
                    break;
                case NP.rigthNull:

                    this.Operator = result => ((string) str.Const.Content).StartsWith(pattern.Operator(result));
                    this.TypedOperator = result => str.Const.Change(o => o.StartsWith(pattern.Operator(result)));
                    this.AggregateLevel = pattern.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = str.Const.Change(o => o.StartsWith(pattern.Const.Content));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
