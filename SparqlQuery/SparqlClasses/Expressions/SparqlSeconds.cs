using System;
using System.Xml;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlSeconds : SparqlExpression
    {
        private SparqlExpression sparqlExpression;

        public SparqlSeconds(SparqlExpression sparqlExpression)
        {
            this.sparqlExpression = sparqlExpression;
            this.Create();
        }

        public void Create()
        {
            this.AggregateLevel = this.sparqlExpression.AggregateLevel;
            Func<dynamic, int> f = r => ((DateTime)r).Second;
            if (this.sparqlExpression.Const != null)
            {
                this.Const = new OV_int(f(this.sparqlExpression.Const.Content));
            }
            else
            {
                this.Operator = result => f(this.sparqlExpression.Operator(result));
                this.TypedOperator = result => new OV_int(this.Operator(result));
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("seconds");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.sparqlExpression.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
