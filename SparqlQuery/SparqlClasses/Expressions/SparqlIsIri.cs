using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlIsIri : SparqlExpression
    {
        

        public SparqlIsIri(SparqlExpression value)
            : base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_bool(value.Const is IIriNode);
            else
            {
                this.Operator = result => value.TypedOperator(result) is IIriNode;
                this.TypedOperator = result => new OV_bool(value.TypedOperator(result) is IIriNode); // todo 
            }

            // SetExprType(ObjectVariantEnum.Bool);
        }
    }
}
