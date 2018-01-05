using System;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlHours : SparqlExpression
    {
        public SparqlHours(SparqlExpression value)
            : base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_int(this.GetHours(value.Const.Content));
            else
            {
                this.Operator = result => GetHours(value.Operator(result));
                this.TypedOperator = result => new OV_int(this.Operator(result));
            }
        }

        private int GetHours(dynamic o)
        {
            if (o is DateTime)
                return ((DateTime)o).Hour;
            if (o is DateTimeOffset)
                return ((DateTimeOffset)o).Hour;
            throw new ArgumentException();
        }
    }
}
