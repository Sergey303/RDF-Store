using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlLangMatches : SparqlExpression
    {
        public SparqlLangMatches(SparqlExpression value, SparqlExpression langExpression)
        {
            // value.SetExprType(ObjectVariantEnum.Str); //todo lang
          // langExpression.SetExprType(ObjectVariantEnum.Str); //todo lang
            switch (NullablePairExt.Get(value.Const, langExpression.Const))
            {
                case NP.bothNull:
                    this.Operator = result =>
            {
                var lang = value.Operator(result);
                var langRange = langExpression.Operator(result);
                return Equals(langRange.Content, "*")
                    ? !string.IsNullOrWhiteSpace(langRange.Content)
                    : Equals(lang, langRange);
            };

                    this.AggregateLevel = SetAggregateLevel(value.AggregateLevel, langExpression.AggregateLevel);
                    break;
                case NP.leftNull:
                    var rlang = langExpression.Const.Content;
                    this.Operator = result =>
                    {
                        var lang = value.Operator(result);
                        return Equals(rlang, "*")
                            ? !string.IsNullOrWhiteSpace(lang)
                            : Equals(lang, rlang);
                    };
                    this.AggregateLevel = value.AggregateLevel;
                    break;
                case NP.rigthNull:
                    var llang = value.Const.Content;
                    if (llang.Equals("*")) this.Operator = result => !string.IsNullOrWhiteSpace(langExpression.Operator(result));
                    else this.Operator = result => Equals(llang, langExpression.Operator(result));
                    this.AggregateLevel = langExpression.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    var ll = value.Const.Content;
                    var rl = langExpression.Const.Content;
                    this.Const = new OV_bool(Equals(rl, "*")
                            ? !string.IsNullOrWhiteSpace((string)ll)
                            : Equals(ll, rl));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.TypedOperator = result =>new OV_bool(this.Operator(result));
        }
    }
}