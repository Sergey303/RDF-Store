using System;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlUuid : SparqlExpression
    {
        public SparqlUuid()    :base(VariableDependenceGroupLevel.UndependableFunc, false)
        {
            this.Operator = this.TypedOperator = result => new OV_iri("urn:uuid:" + Guid.NewGuid());
        }
    }
}
