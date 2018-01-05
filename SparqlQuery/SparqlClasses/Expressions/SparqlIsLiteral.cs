using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlIsLiteral : SparqlExpression
    {
        public SparqlIsLiteral(SparqlExpression value)
            : base(value.AggregateLevel, value.IsStoreUsed)
        {
            // SetExprType(ObjectVariantEnum.Bool);
            if (value.Const != null) this.Const = new OV_bool(value.Const is ILiteralNode);
            else
            {
                this.Operator = result => value.TypedOperator(result) is ILiteralNode;
                this.TypedOperator = result => new OV_bool(value.TypedOperator(result) is ILiteralNode); // todo 
            }
        }
    }
}
