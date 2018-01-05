using System;

namespace SparqlQuery.SparqlClasses.Expressions
{

    public class SparqlAbs : SparqlUnaryExpression
    {                                    
        public SparqlAbs(SparqlExpression value)
            : base(o => Math.Abs(o), value, "abs")
        {
           // value.SetExprType(ExpressionTypeEnum.numeric);
            // SetExprType(ExpressionTypeEnum.numeric);
        }
    }
}