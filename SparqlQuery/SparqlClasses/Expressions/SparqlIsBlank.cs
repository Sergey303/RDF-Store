using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlIsBlank : SparqlExpression
    {
      

        public SparqlIsBlank(SparqlExpression value)  :base(value.AggregateLevel, value.IsStoreUsed)
        {
            // SetExprType(ObjectVariantEnum.Bool);
            if(value.Const!=null) this.Const=new OV_bool(value.Const is IBlankNode);
            else
            {
                this.Operator = result => value.TypedOperator(result) is IBlankNode;
                this.TypedOperator = result => new OV_bool(value.TypedOperator(result) is IBlankNode); // todo     
            }
            
        }
    }
}
