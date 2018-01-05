using System;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlRand : SparqlExpression
    {
        static Random r = new Random();
        public SparqlRand()  : base(VariableDependenceGroupLevel.UndependableFunc, false)
        {
            this.Operator = res => r.NextDouble();
            this.TypedOperator = result => new OV_double(r.NextDouble());
        }
    }
}