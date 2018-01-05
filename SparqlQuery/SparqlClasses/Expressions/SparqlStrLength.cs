using RDFCommon.OVns;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlStrLength  : SparqlExpression
    {
        public SparqlStrLength(SparqlExpression value) :base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_int(((string) value.Const.Content).Length);
            else
            {
                this.Operator = result => ((string) value.TypedOperator(result).Content).Length;
                this.TypedOperator = result => new OV_int(((string) value.TypedOperator(result).Content).Length);
            }
        }
    }
}
