using System.Xml;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlNumLiteralExpression : SparqlExpression
    {
        public SparqlNumLiteralExpression(ObjectVariants sparqlLiteralNode)
        {
            // SetExprType(ExpressionTypeEnum.numeric);
            this.Const = sparqlLiteralNode;

            // TypedOperator = result => sparqlLiteralNode;
        }

        public override void WriteXml(XmlWriter writer)
        {
            this.Const.WriteXml(writer);
        }
    }
}
