using System;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlYear : SparqlUnaryExpression<OV_int>
    {
        public SparqlYear(SparqlExpression value)
            :base(f =>
            {
                if (f is DateTime)
                    return new OV_int(((DateTime) f).Year);
                if (f is DateTimeOffset)
                    return new OV_int(((DateTimeOffset) f).Year);
                throw new ArgumentException();
            }, value, y=> new OV_int(y))
        {                                            
            
        }
    }
}
