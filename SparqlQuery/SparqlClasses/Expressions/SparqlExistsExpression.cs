using System.Linq;
using System.Xml;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using SparqlQuery.SparqlClasses.GraphPattern;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlExistsExpression :SparqlExpression
    {
        private readonly ISparqlGraphPattern _sparqlGraphPattern;


        public SparqlExistsExpression(ISparqlGraphPattern sparqlGraphPattern)   : base(VariableDependenceGroupLevel.SimpleVariable, true)
        {
            this._sparqlGraphPattern = sparqlGraphPattern;

            // TODO: Complete member initialization

            // SetExprType(ObjectVariantEnum.Bool);
            this.Operator = variableBinding => sparqlGraphPattern.Run(Enumerable.Repeat(variableBinding, 1)).Any();
            this.TypedOperator = variableBinding => new OV_bool(this.Operator(variableBinding));
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
