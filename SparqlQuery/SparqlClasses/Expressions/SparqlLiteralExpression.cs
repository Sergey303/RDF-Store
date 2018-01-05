using System.Xml;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlLiteralExpression : SparqlExpression
    {

        public SparqlLiteralExpression(ObjectVariants sparqlLiteralNode)
        {
            this.Const = sparqlLiteralNode;

            // SetExprType(sparqlLiteralNode.Variant);
        }

        public override void WriteXml(XmlWriter writer)
        {
            this.Const.WriteXml(writer);
        }
    }
}
