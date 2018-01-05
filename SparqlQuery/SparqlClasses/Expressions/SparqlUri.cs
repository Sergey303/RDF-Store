using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlUri : SparqlExpression
    {
        public SparqlUri(SparqlExpression value, RdfQuery11Translator q)  :base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_iri(q.prolog.GetFromString((string)value.Const.Content));
            else
            {
                this.Operator = result => new OV_iri(q.prolog.GetFromString((string)value.Operator(result).Content));

                // SetExprType(ObjectVariantEnum.Iri);
                this.TypedOperator =
                    result => new OV_iri(q.prolog.GetFromString((string)value.TypedOperator(result).Content));
            }
        }
    }
}
