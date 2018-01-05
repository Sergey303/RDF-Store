using System;
using System.Xml;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlOrExpression : SparqlExpression
    {
        private readonly SparqlExpression _l;
        private readonly SparqlExpression _r;

        public SparqlOrExpression(SparqlExpression l, SparqlExpression r)
        {
            this._l = l;
            this._r = r;

            // l.SetExprType(ObjectVariantEnum.Bool);
            // r.SetExprType(ObjectVariantEnum.Bool);
            // SetExprType(ObjectVariantEnum.Bool);         
            switch (NullablePairExt.Get(l.Const, r.Const))
            {
                case NP.bothNull:
                    this.Operator = res => l.Operator(res) || r.Operator(res);
                    this.AggregateLevel = SetAggregateLevel(l.AggregateLevel, r.AggregateLevel);
                    break;
                case NP.leftNull:
                    if ((bool)l.Const.Content) this.Const = new OV_bool(true);
                    else
                    {
                        this.Operator = r.Operator;
                        this.AggregateLevel = r.AggregateLevel;
                    }

                    break;
                case NP.rigthNull:
                    if ((bool)r.Const.Content) this.Const = new OV_bool(true);
                    else
                    {
                        this.Operator = l.Operator;
                        this.AggregateLevel = l.AggregateLevel;
                    }

                    break;
                case NP.bothNotNull:
                    this.Const = new OV_bool((bool)l.Const.Content || (bool)r.Const.Content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.TypedOperator = result => new OV_bool(this.Operator(result));
        }

        public static SparqlExpression Create(SparqlExpression l, SparqlExpression r)
        {
            return new SparqlOrExpression(l, r);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("or");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this._l.WriteXml(writer);
            this._r.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
