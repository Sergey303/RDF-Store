using System;
using RDFCommon.OVns;
using RDFCommon.OVns.date;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlNow : SparqlExpression
    {
        public SparqlNow()    :base(VariableDependenceGroupLevel.UndependableFunc, false)
        {
            this.Operator = res => DateTime.UtcNow;
            this.TypedOperator=res=>new OV_dateTime(DateTime.UtcNow);
        }
    }
}
