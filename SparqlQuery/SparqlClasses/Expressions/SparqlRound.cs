using System;

namespace SparqlQuery.SparqlClasses.Expressions
{
    internal class SparqlRound : SparqlUnaryExpression
    {
        public SparqlRound(SparqlExpression value) : base(o => Math.Round(o), value, "round")
        {

        }
    }
}