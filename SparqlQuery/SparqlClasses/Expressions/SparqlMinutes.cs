using System;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlMinutes : SparqlExpression
    {
        public SparqlMinutes(SparqlExpression value):base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_int(GetMinute(value.Const.Content));
            else
            {
                this.Operator = result => GetMinute(value.Operator(result));
                this.TypedOperator = result => new OV_int(this.Operator(result));
            }
        }

        private static dynamic GetMinute(dynamic o)
        {
            if (o is DateTime)
                return ((DateTime) o).Minute;
            if (o is DateTimeOffset)
                return ((DateTimeOffset) o).Minute;
            throw new ArgumentException();
        }
    }
}
