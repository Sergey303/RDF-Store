using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlStrUuid : SparqlExpression
    {
        public SparqlStrUuid()      :base(VariableDependenceGroupLevel.UndependableFunc, false)
        {
            this.TypedOperator = result => new OV_string(Guid.NewGuid().ToString());
            this.Operator = result => Guid.NewGuid().ToString();
        }
    }
}
