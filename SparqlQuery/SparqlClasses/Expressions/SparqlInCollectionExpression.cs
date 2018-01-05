using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlInCollectionExpression : SparqlExpression
    {
        private readonly SparqlExpression _itemExpression;
        private readonly List<SparqlExpression> _collection;

        public SparqlInCollectionExpression (SparqlExpression itemExpression, List<SparqlExpression> collection)
        {
            this._itemExpression = itemExpression;
            this._collection = collection;
            var cConsts = collection.Select(expression => expression.Const).ToArray();
            if (itemExpression.Const != null)
            {
                if (cConsts.Contains(itemExpression.Const))
                {
                    this.Const = new OV_bool(true);
                    return;
                }

                if (cConsts.All(c => c != null))
                {
                    this.Const = new OV_bool(false);
                    return;
                }

                this.AggregateLevel = SetAggregateLevel(collection.Select(c => c.AggregateLevel).ToArray());
            }
            else
                this.AggregateLevel = SetAggregateLevel(
                    itemExpression.AggregateLevel,
                    SetAggregateLevel(collection.Select(c => c.AggregateLevel).ToArray()));

            this.Operator = result =>
                {
                    var o = itemExpression.Operator(result);
                    return collection.Any(element => element.Operator(result).Equals(o));
                };
            this.TypedOperator = o => new OV_bool(this.Operator(o));

            // SetExprType(ObjectVariantEnum.Bool);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("inCollection");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this._itemExpression.WriteXml(writer);
            foreach (var colItem in this._collection)
                colItem.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}