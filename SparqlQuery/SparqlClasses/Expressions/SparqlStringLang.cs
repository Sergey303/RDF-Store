using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlStringLang  :SparqlExpression
    {
        public SparqlStringLang(SparqlExpression literalExpression, SparqlExpression langExpression)
        {
            switch (NullablePairExt.Get(literalExpression.Const, langExpression.Const))
            {
                case NP.bothNull:
                    this.Operator = literalExpression.Operator;
                    this.TypedOperator = result => new OV_langstring((string)literalExpression.Operator(result), (string)langExpression.Operator(result));
                    this.AggregateLevel = SetAggregateLevel(literalExpression.AggregateLevel, langExpression.AggregateLevel);
                    break;
                case NP.leftNull:
                    this.Operator = literalExpression.Operator;
                    this.TypedOperator = result => new OV_langstring((string)literalExpression.Operator(result), (string) langExpression.Const.Content);
                    this.AggregateLevel = literalExpression.AggregateLevel;
                    break;
                case NP.rigthNull:
                    this.Operator = result => literalExpression.Const.Content;
                    this.TypedOperator = result => new OV_langstring((string) literalExpression.Const.Content, langExpression.Operator(result));
                    this.AggregateLevel = langExpression.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = new OV_langstring((string) literalExpression.Const.Content, (string) langExpression.Const.Content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}
