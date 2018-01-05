using System;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlFloor : SparqlUnaryExpression
    {
        public SparqlFloor(SparqlExpression value)
            : base(d => Math.Floor(d), value, "floor")
            
        {
           
        }
    }
}