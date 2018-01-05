using System.Linq;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SparqlAggregateExpression
{
    using System.Xml;

    class SparqlGroupConcatExpression : SparqlAggregateExpression
    {
        protected override void Create()
        {
            if (this.Expression.Const != null)
            {
                this.Operator = result => string.Join(this.Separator, ((SparqlGroupOfResults)result).Group.Select(r=> this.Expression.Const.Content));
                this.TypedOperator = result => new OV_string(string.Join(this.Separator, ((SparqlGroupOfResults)result).Group.Select(r=> this.Expression.Const.Content)));
            }
            else
            {
                this.Operator = result => string.Join(this.Separator, ((SparqlGroupOfResults) result).Group.Select(this.Expression.Operator));
                this.TypedOperator = result => new OV_string(string.Join(this.Separator, ((SparqlGroupOfResults)result).Group.Select(this.Expression.Operator)));
            }
        }
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("concat");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
        }
    }
}
