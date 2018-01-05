using System.Linq;
using System.Xml;
using SparqlQuery.SparqlClasses.GraphPattern;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlNotExistsExpression : SparqlExistsExpression
    {
        private readonly ISparqlGraphPattern _sparqlGraphPattern;

        public SparqlNotExistsExpression(ISparqlGraphPattern sparqlGraphPattern)
            : base(sparqlGraphPattern)
        {
            this._sparqlGraphPattern = sparqlGraphPattern;

            // TODO: Complete member initialization
            // this.sparqlGraphPattern = sparqlGraphPattern;
            this.Operator = variableBinding => !sparqlGraphPattern.Run(Enumerable.Repeat(variableBinding, 1)).Any();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("exist");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this._sparqlGraphPattern.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
