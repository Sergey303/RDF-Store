using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlSameTerm : SparqlExpression
    {
        public SparqlSameTerm(SparqlExpression str, SparqlExpression pattern)
        {
            switch (NullablePairExt.Get(str.Const, pattern.Const))
            {
                case NP.bothNull:
                    this.Operator = result => str.TypedOperator(result).Equals(pattern.TypedOperator(result));
                    this.AggregateLevel = SetAggregateLevel(str.AggregateLevel, pattern.AggregateLevel);
                    break;
                case NP.leftNull:
                    this.Operator = result => str.TypedOperator(result).Equals(pattern.Const);
                    this.AggregateLevel = str.AggregateLevel;
                    break;
                case NP.rigthNull:
                    this.Operator = result => str.Const.Equals(pattern.TypedOperator(result));
                    this.AggregateLevel = pattern.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = new OV_bool(str.Const.Equals(pattern.Const));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.TypedOperator = result => new OV_bool(this.Operator(result));
        }
    }
}
