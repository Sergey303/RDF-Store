using System.Xml;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlBoolLiteralExpression : SparqlExpression
    {

        public SparqlBoolLiteralExpression(ObjectVariants sparqlLiteralNode)
        {
            this.Const = sparqlLiteralNode;
            
        }

        public override void WriteXml(XmlWriter writer)
        {
            this.Const.WriteXml(writer);
        }
    }

}
