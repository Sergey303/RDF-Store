using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlToString : SparqlExpression
    {
       // private SparqlExpression sparqlExpression;
        public SparqlToString(SparqlExpression child):base(child.AggregateLevel, child.IsStoreUsed)
           
        {
            var childConst = child.Const;
            if (childConst != null) this.Const =new OV_string(childConst.Content.ToString());
            else
            {
                this.Operator = result => child.Operator(result).ToString();
                this.TypedOperator = result => new OV_string(child.Operator(result).ToString());                
               
            }
        }
    }
}
