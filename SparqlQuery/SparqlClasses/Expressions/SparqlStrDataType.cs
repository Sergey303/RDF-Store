using System;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlStrDataType : SparqlExpression
    {
     

        public SparqlStrDataType(SparqlExpression sparqlExpression1, SparqlExpression sparqlExpression2, NodeGenerator q)
        {
            // TODO: Complete member initialization
            switch (NullablePairExt.Get(sparqlExpression1.Const, sparqlExpression2.Const))
            {
                case NP.bothNull:
                    this.TypedOperator = result => q.CreateLiteralNode((string)sparqlExpression1.Operator(result), (string)sparqlExpression2.Operator(result));
                    this.Operator = res => sparqlExpression1.Operator(res);
                    this.AggregateLevel = SetAggregateLevel(sparqlExpression1.AggregateLevel, sparqlExpression2.AggregateLevel);
                    break;
                case NP.leftNull:
                    this.TypedOperator = result => q.CreateLiteralNode((string)sparqlExpression1.Operator(result), (string)sparqlExpression2.Const.Content);
                    this.Operator = res => sparqlExpression1.Operator(res);
                    this.AggregateLevel = sparqlExpression1.AggregateLevel;
                    break;
                case NP.rigthNull:
                    this.TypedOperator = result => q.CreateLiteralNode((string)sparqlExpression1.Const.Content, (string)sparqlExpression2.Operator(result));
                    this.Operator = res => sparqlExpression1.Const.Content;
                    this.AggregateLevel = sparqlExpression2.AggregateLevel;

                    break;
                case NP.bothNotNull:
                    this.Const = q.CreateLiteralNode((string)sparqlExpression1.Const.Content, (string)sparqlExpression2.Const.Content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
