using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlStrBefore : SparqlExpression
    {
        public SparqlStrBefore(SparqlExpression str, SparqlExpression pattern)
        {
            switch (NullablePairExt.Get(str.Const, pattern.Const))
            {
                case NP.bothNull:
                    this.Operator = result => StringBefore(str.Operator(result), pattern.Operator(result));
                    this.AggregateLevel = SetAggregateLevel(str.AggregateLevel, pattern.AggregateLevel);
                    this.TypedOperator = result => str.TypedOperator(result).Change(o => StringBefore(o, (string)pattern.TypedOperator(result).Content));
                    break;
                case NP.leftNull:
                    this.Operator = result => StringBefore(str.Operator(result), (string)pattern.Const.Content);
                    this.TypedOperator = result => pattern.Const.Change(o => StringBefore(str.Operator(result), o));
                    this.AggregateLevel = str.AggregateLevel;
                    break;
                case NP.rigthNull:
                    this.Operator = result => StringBefore((string)str.Const.Content, pattern.Operator(result));
                    this.TypedOperator = result => str.Const.Change(o => StringBefore(o, (string)pattern.Operator(result).Content));
                    this.AggregateLevel = pattern.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = new OV_bool(this.StringBefore((string)str.Const.Content, (string)pattern.Const.Content));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

          string StringBefore(string str, string pattern)
        {
            if (pattern == string.Empty) return string.Empty;
           int index = str.LastIndexOf(pattern, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1 || (index += pattern.Length )>= str.Length) return string.Empty;
            return str.Substring(index);
        }
    }
}
