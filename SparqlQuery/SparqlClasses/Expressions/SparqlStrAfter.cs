using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlStrAfter : SparqlExpression
    {         
        public SparqlStrAfter(SparqlExpression str, SparqlExpression pattern)
        {
      
            // TODO: Complete member initialization
            // str.SetExprType(ExpressionTypeEnum.@stringOrWithLang);
           // pattern.SetExprType(ExpressionTypeEnum.@stringOrWithLang);

           // SetExprType(ExpressionTypeEnum.@stringOrWithLang);
            switch (NullablePairExt.Get(str.Const, pattern.Const))
            {
                case NP.bothNull:
                    this.Operator = result => StringAfter(str.Operator(result), pattern.Operator(result));
                    this.AggregateLevel = SetAggregateLevel(str.AggregateLevel, pattern.AggregateLevel);
                    this.TypedOperator = result => str.TypedOperator(result).Change(o => StringAfter(o, (string)pattern.TypedOperator(result).Content));
                    break;
                case NP.leftNull:
                    this.Operator = result => StringAfter(str.Operator(result), (string) pattern.Const.Content);
                    this.TypedOperator = result => pattern.Const.Change(o => StringAfter(str.Operator(result), o));
                    this.AggregateLevel = str.AggregateLevel;
                    break;
                case NP.rigthNull:
                    this.Operator = result => StringAfter((string) str.Const.Content, pattern.Operator(result));
                    this.TypedOperator = result => str.Const.Change(o => StringAfter(o, (string)pattern.Operator(result).Content));
                    this.AggregateLevel = pattern.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = new OV_bool(this.StringAfter((string) str.Const.Content, (string) pattern.Const.Content));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
          
        }

        string StringAfter(string str, string pattern)
        {
            if (Equals(pattern, string.Empty)) return string.Empty;
           int index = str.LastIndexOf(pattern, StringComparison.InvariantCultureIgnoreCase);
            if (Equals(index, -1) || (index += pattern.Length )>= str.Length) return string.Empty;
            return str.Substring(index);
        }
    }
}
