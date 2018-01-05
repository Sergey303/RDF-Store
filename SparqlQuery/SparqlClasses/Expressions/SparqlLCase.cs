namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlLCase :SparqlExpression
    {
        public SparqlLCase(SparqlExpression value)        :base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = value.Const.Change(o => o.ToLower());
            else
            {
                this.Operator = result => value.Operator(result).ToLower();
                this.TypedOperator = result => value.TypedOperator(result).Change(o => o.ToLower());
            }
        }
    }
}
