using System;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlDay : SparqlExpression
    {
        public SparqlDay(SparqlExpression value)
            : base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_int(this.GetDay(value.Const.Content));
            else
            {
                this.Operator = result => GetDay(value.Operator(result));
                this.TypedOperator = result => new OV_int(this.Operator(result));
            }
        }

        private int GetDay(dynamic o)
        {
            if (o is DateTime)
                return ((DateTime)o).Day;
            if (o is DateTimeOffset)
                return ((DateTimeOffset)o).Day;
            throw new ArgumentException();
        }
    }
}
