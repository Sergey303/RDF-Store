using System.Xml;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlIriExpression : SparqlExpression
    {
        public SparqlIriExpression(string sparqlUriNode, NodeGenerator q) 
        {
            // TypedOperator = result => uri;
            this.Const = q.GetUri(sparqlUriNode);
        }

        public SparqlIriExpression(ObjectVariants sparqlUriNode)
        {
            this.Const = sparqlUriNode;
        }

        public override void WriteXml(XmlWriter writer)
        {
            this.Const.WriteXml(writer);
        }
    }
}
