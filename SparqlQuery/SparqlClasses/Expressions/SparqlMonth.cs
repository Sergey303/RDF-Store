using System;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlMonth : SparqlExpression
    {
     
        public SparqlMonth(SparqlExpression value)  :base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_int(this.GetMonth(value.Const.Content));
            else
            {
                this.Operator = result => GetMonth(value.Operator(result));
                this.TypedOperator = result => new OV_int(this.Operator(result));
            }
        }

        private int GetMonth(dynamic o)
        {
            if (o is DateTime)
                return ((DateTime)o).Month;
            if (o is DateTimeOffset)
                return ((DateTimeOffset)o).Month;
            throw new ArgumentException();
        }
    }
}
