using System.Xml;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    abstract class SparqlHashExpression : SparqlExpression
    {
        private readonly SparqlExpression _value;

        public SparqlHashExpression(SparqlExpression value)
            : base(value.AggregateLevel, value.IsStoreUsed)
        {
            this._value = value;
        }

        protected void Create(SparqlExpression value)
        {
            if (value.Const != null) this.Const = new OV_string(this.CreateHash((string) value.Const.Content));
            else
            {
                this.Operator = result => CreateHash(value.Operator(result));
                this.TypedOperator = result => new OV_string(this.Operator(result));
            }
        }

        protected abstract string CreateHash(string f);

        public override void WriteXml(XmlWriter writer)
        {
            this._value.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}