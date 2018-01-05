namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlBnode : SparqlExpression
    {

        public SparqlBnode(SparqlExpression value, RdfQuery11Translator q)       :base(value.AggregateLevel, value.IsStoreUsed)
        {
            // IsDistinct = value.IsDistinct;
            // value.SetExprType(ObjectVariantEnum.Str);
            // SetExprType(ObjectVariantEnum.Iri);
            var litConst = value.Const;
            if (litConst != null)
            {
                this.Operator = this.TypedOperator = result => q.Store.NodeGenerator.CreateBlankNode(value.Const.Content + result.Id);

            }
            else
            {
                this.Operator =
                    this.TypedOperator =
                        result => q.Store.NodeGenerator.CreateBlankNode(value.Operator(result) + result.Id);

                // OvConstruction = o => q.Store.NodeGenerator.CreateBlankNode((string) o);
            }
        }

        public SparqlBnode(RdfQuery11Translator q):base(VariableDependenceGroupLevel.UndependableFunc, true)
        {
          // SetExprType(ObjectVariantEnum.Iri);
            // OvConstruction = o => q.Store.NodeGenerator.CreateBlankNode(); 
            this.Operator = this.TypedOperator = result => q.Store.NodeGenerator.CreateBlankNode();
        }                      
    }
}
