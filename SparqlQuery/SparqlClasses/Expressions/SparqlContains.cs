using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlContains : SparqlExpression
    {
        public SparqlContains(SparqlExpression str, SparqlExpression pattern)
        {
            // SetExprType(ObjectVariantEnum.Bool);
            // str.SetExprType(ExpressionTypeEnum.stringOrWithLang); str.SetExprType(ExpressionTypeEnum.stringOrWithLang);
            // pattern.SetExprType(ExpressionTypeEnum.stringOrWithLang); str.SetExprType(ExpressionTypeEnum.stringOrWithLang);
            if (str.Const != null)
            {
                if (pattern.Const != null) this.Const = new OV_bool(((string) str.Const.Content).Contains((string) pattern.Const.Content));
                else
                {
                    this.Operator = result =>
                    {
                        var ps = pattern.TypedOperator(result);
                        if ((str.Const is OV_langstring && ps is OV_langstring)
                            || (str.Const is OV_langstring && ps is OV_string)
                            || (str.Const is OV_string && ps is OV_string))
                            return ((string) str.Const.Content).Contains((string) ps.Content);
                        throw new ArgumentException();
                    };
                    this.TypedOperator = result => new OV_bool(this.Operator(result));
                }
            }
            else 
            {
                if (pattern.Const != null)
                {
                    this.Operator = result =>
                    {
                        var s = str.TypedOperator(result);
                        if ((s is OV_langstring && pattern.Const is OV_langstring)
                            || (s is OV_langstring && pattern.Const is OV_string)
                            || (s is OV_string && pattern.Const is OV_string))
                            return ((string)s.Content).Contains((string)pattern.Const.Content);
                        throw new ArgumentException();
                    };
                }
                else
                {
                    this.Operator = result =>
                    {
                        var s = str.TypedOperator(result);
                        var ps = pattern.TypedOperator(result);
                        if ((s is OV_langstring && ps is OV_langstring)
                            || (s is OV_langstring && ps is OV_string)
                            || (s is OV_string && ps is OV_string))
                            return ((string)s.Content).Contains((string)ps.Content);
                        throw new ArgumentException();
                    };
                }

                this.TypedOperator = result => new OV_bool(this.Operator(result));
            }

            this.AggregateLevel = SetAggregateLevel(str.AggregateLevel, pattern.AggregateLevel);
        }
    }
}
