using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlStrEnds : SparqlExpression
    {

        public SparqlStrEnds(SparqlExpression str, SparqlExpression pattern)
        {
            switch (NullablePairExt.Get(str.Const, pattern.Const))
            {
                case NP.bothNull:
                    this.Operator = result => ((string)str.Operator(result)).EndsWith(pattern.Operator(result));
                    this.AggregateLevel = SetAggregateLevel(str.AggregateLevel, pattern.AggregateLevel);
                    this.TypedOperator = result => str.TypedOperator(result).Change(o => ((string)o).EndsWith((string)pattern.TypedOperator(result).Content));
                    break;
                case NP.leftNull:
                    this.Operator = result => ((string)str.Operator(result)).EndsWith((string)pattern.Const.Content);
                    this.TypedOperator = result => pattern.Const.Change(o => ((string)str.Operator(result)).EndsWith(o));
                    this.AggregateLevel = str.AggregateLevel;
                    break;
                case NP.rigthNull:
                    this.Operator = result => ((string)str.Const.Content).EndsWith(pattern.Operator(result));
                    this.TypedOperator = result => str.Const.Change(o => ((string)o).EndsWith((string)pattern.Operator(result).Content));
                    this.AggregateLevel = pattern.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = new OV_bool(((string)str.Const.Content).EndsWith((string)pattern.Const.Content));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }
}
