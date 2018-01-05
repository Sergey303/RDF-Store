using System;
using System.Xml;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    internal class SparqlNotEqualsExpression : SparqlExpression
    {
        private readonly SparqlExpression _l;
        private readonly SparqlExpression _r;

        public SparqlNotEqualsExpression(SparqlExpression l, SparqlExpression r, NodeGenerator ng)
        {
            this._l = l;
            this._r = r;

            var lc = l.Const;
            var rc = r.Const;
             
            switch (NullablePairExt.Get(lc, rc))
            {
                case NP.bothNull:
                    this.Operator = result => !l.TypedOperator(result).Equals(r.TypedOperator(result));
                    this.AggregateLevel = SetAggregateLevel(l.AggregateLevel, r.AggregateLevel);
                    break;
                case NP.leftNull:
                    if (rc.Variant == ObjectVariantEnum.Iri)
                    {
                        OV_iriint rcCoded = null;
                        this.Operator = result =>
                        {
                            var lVal = l.TypedOperator(result);
                            if (lVal.Variant == ObjectVariantEnum.Iri)
                                return !lVal.Equals(rc);
                            if (lVal.Variant == ObjectVariantEnum.IriInt)
                            {
                                if (rcCoded == null)
                                    rcCoded = (OV_iriint) ng.GetUri((string) rc.Content);
                                return ((OV_iriint) lVal).code != rcCoded.code;
                            }
                            else throw new AggregateException();
                        };
                    }
                    else this.Operator = result => !l.TypedOperator(result).Equals(rc);

                    this.AggregateLevel = l.AggregateLevel;
                    break;
                case NP.rigthNull:
                    if (lc.Variant == ObjectVariantEnum.Iri)
                    {
                        OV_iriint lcCoded = null;
                        this.Operator = result =>
                        {
                            var rVal = r.TypedOperator(result);
                            if (rVal.Variant == ObjectVariantEnum.Iri)
                                return !rVal.Equals(lc);
                            if (rVal.Variant == ObjectVariantEnum.IriInt)
                            {
                                if (lcCoded == null)
                                    lcCoded = (OV_iriint)ng.GetUri((string)lc.Content);
                                return ((OV_iriint)rVal).code != lcCoded.code;
                            }
                            else throw new AggregateException();
                        };
                    }
                    else this.Operator = result => !lc.Equals(r.TypedOperator(result));
                    this.AggregateLevel = r.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = new OV_bool(!lc.Equals(rc));
                    break;
            }

            this.TypedOperator = result => new OV_bool(this.Operator(result));

        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("notEqual");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this._l.WriteXml(writer);
            this._r.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}