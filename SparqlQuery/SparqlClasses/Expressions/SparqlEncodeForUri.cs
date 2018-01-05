using System.Web;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlEncodeForUri : SparqlExpression
    {
        public SparqlEncodeForUri(SparqlExpression value, RdfQuery11Translator q)   :base(value.AggregateLevel, value.IsStoreUsed)
        {
            // SetExprType(ObjectVariantEnum.Str);
          // value.SetExprType(ExpressionTypeEnum.stringOrWithLang);
            if (value.Const != null) this.Const = new OV_string(HttpUtility.UrlEncode((string) value.Const.Content));
            else
            {
                this.Operator = result => HttpUtility.UrlEncode((string) value.Operator(result).Content);
                this.TypedOperator = result => new OV_string(this.Operator(result));
            }
        }
    }
}
