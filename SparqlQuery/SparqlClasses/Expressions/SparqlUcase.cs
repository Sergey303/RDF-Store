namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlUcase :SparqlUnaryExpression
    {
        public SparqlUcase(SparqlExpression value)
            : base(o => o.ToUpperInvariant(), value, "ucase")
        {

        }
    }
}
