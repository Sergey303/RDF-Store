using System;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlDataType : SparqlExpression
    {
        public SparqlDataType(SparqlExpression value)     : base(value.AggregateLevel, value.IsStoreUsed)
        {
            // SetExprType(ObjectVariantEnum.Iri);
        // value.SetExprType(ExpressionTypeEnum.literal);
            if (value.Const != null)
            {
                this.Const = new OV_iri(((ILiteralNode) value.Const).DataType);
            }
            else
            {
                this.Operator= this.TypedOperator = result =>
                {
                    var r = value.TypedOperator(result);
                    var literalNode = r as ILiteralNode;
                    if (literalNode != null)
                        return new OV_iri(literalNode.DataType);
                    throw new ArgumentException();
                };
            }
        }
    }
}