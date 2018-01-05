using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using RDFCommon.OVns.other;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlLang  : SparqlExpression
    {
        public SparqlLang(SparqlExpression value)  :base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_string(((ILanguageLiteral) value.Const).Lang.Substring(1));
            else
            {
                this.TypedOperator = result => new OV_language(this.Operator(result));
                this.Operator= result => 
                {
                    var f = value.TypedOperator(result);
                        return new OV_string(((ILanguageLiteral) f).Lang.Substring(1));
                };
            }
        }
    }
}
