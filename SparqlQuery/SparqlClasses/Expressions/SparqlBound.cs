using RDFCommon.OVns;
using RDFCommon.OVns.general;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlBound : SparqlExpression
    {
        public SparqlBound(VariableNode value)           :base(VariableDependenceGroupLevel.SimpleVariable, false)
        {
            // SetExprType(ObjectVariantEnum.Bool);
            this.Operator = result => result.ContainsKey(value);
            this.TypedOperator = result => new OV_bool(result.ContainsKey(value));
        }

   
    }
}