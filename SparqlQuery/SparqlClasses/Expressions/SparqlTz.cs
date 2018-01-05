using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlTz : SparqlExpression
    {
        public SparqlTz(SparqlExpression value)   :base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null)
            {
                var f = value.Const.Content;

                if (f is DateTimeOffset) this.Const = new OV_string(((DateTimeOffset) f).Offset.ToString());
                else if (f is DateTime) this.Const = new OV_string(string.Empty);
                throw new ArgumentException();
            }
            else
            {
                this.Operator = result =>
                {
                    var f = value.Operator(result);
                    if (f is DateTimeOffset)
                    {
                        return ((DateTimeOffset) f).Offset.ToString();
                    }
                    else if (f is DateTime) return string.Empty;
                    throw new ArgumentException();
                };
                this.TypedOperator = result => new OV_string(this.Operator(result));
            }
        }

       
    }
}
