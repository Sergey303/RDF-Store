using System;
using System.Xml;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlBinaryExpression : SparqlExpression
    {
        private readonly SparqlExpression l;
        private readonly SparqlExpression r;
        private readonly Func<dynamic, dynamic, dynamic> @operator;
        private readonly string _symbol;

        protected internal SparqlBinaryExpression(SparqlExpression l, SparqlExpression r, Func<dynamic, dynamic, dynamic> @operator, string symbol)
        {
            this.l = l;
            this.r = r;
          this. @operator = @operator;
            this._symbol = symbol;
        }

        public virtual void Create()
        {
            var lc = this.l.Const;
            var rc = this.r.Const;

            switch (NullablePairExt.Get(lc, rc))
            {
                case NP.bothNull:
                    this.Operator = result => this.@operator(this.l.Operator(result), this.r.Operator(result));
                    this.TypedOperator = result => this.ApplyTyped(this.l.TypedOperator(result), this.r.TypedOperator(result));
                    this.AggregateLevel = SetAggregateLevel(this.l.AggregateLevel, this.r.AggregateLevel);
                    break;
                case NP.leftNull:
                    this.Operator = result => this.@operator(this.l.Operator(result), rc.Content);
                    this.TypedOperator = result => this.ApplyTyped(this.l.TypedOperator(result),  rc);
                    this.AggregateLevel = this.l.AggregateLevel;
                    break;
                case NP.rigthNull:
                    this.Operator = result => this.@operator(lc.Content, this.r.Operator(result));
                    this.TypedOperator = result => this.ApplyTyped(lc, this.r.TypedOperator(result));
                    this.AggregateLevel = this.r.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = this.ApplyTyped(lc, rc);
                    break;
            }
        }

        protected virtual ObjectVariants ApplyTyped(ObjectVariants left, ObjectVariants right)
        {
            if (left.Variant == ObjectVariantEnum.Typed && right.Variant == ObjectVariantEnum.Typed)
            {
                // double dl,dr;
                int il, ir;
                long ll, lr;
                if (int.TryParse((string) left.Content, out il) && int.TryParse((string) right.Content, out ir))
                    return new OV_typed(this.@operator(il, ir).ToString(), ((OV_typed) left).turi);
                if (long.TryParse((string) left.Content, out ll) && long.TryParse((string) left.Content, out lr))
                    return new OV_typed(this.@operator(ll, lr).ToString(), ((OV_typed) left).turi);
                else
                    return
                        new OV_typed(this.@operator(Convert.ToDouble(left.Content), Convert.ToDouble(right.Content)).ToString(),
                            ((OV_typed) left).turi);

            }
            else if (left.Variant == ObjectVariantEnum.Typed || right.Variant == ObjectVariantEnum.Typed)
            {
                OV_typed t;
                ObjectVariants second;
                if ((left as OV_typed) != null)
                {
                    t = left as OV_typed;
                    second = right;
                }
                else
                {
                    t = right as OV_typed;
                    second = left;
                }

                // double d;
                int i;
                long l;
                if (int.TryParse((string)t.Content, out i) && second.Content is int)
                    return
                        new OV_typed(
                            this.@operator(Convert.ToInt32(left.Content), Convert.ToInt32(right.Content)).ToString(),
                            t.turi);
                if (long.TryParse((string)t.Content, out l) && second.Content is long)
                    return
                        new OV_typed(
                            this.@operator(Convert.ToInt64(left.Content), Convert.ToInt64(right.Content)).ToString(),
                            t.turi);
                else
                    return
                        new OV_typed(
                            this.@operator(Convert.ToDouble(left.Content), Convert.ToDouble(right.Content)).ToString(),
                            t.turi);
            }
            else
            {
                if (left is INumLiteral && right is INumLiteral)
                {
                    if (left.Variant == ObjectVariantEnum.Double || right.Variant == ObjectVariantEnum.Double)
                    {
                        return
                            new OV_double(
                                this.@operator(Convert.ToDouble(left.Content), Convert.ToDouble(right.Content)));
                    }
                    else if (left.Variant == ObjectVariantEnum.Decimal || right.Variant == ObjectVariantEnum.Decimal)
                    {
                        return
                            new OV_decimal(
                                this.@operator(Convert.ToDecimal(left.Content), Convert.ToDecimal(right.Content)));
                    }
                    else if (left.Variant == ObjectVariantEnum.Float || right.Variant == ObjectVariantEnum.Float)
                    {
                        return
                            new OV_float(
                                this.@operator(Convert.ToSingle(left.Content), Convert.ToSingle(right.Content)));
                    }
                    else if (left.Variant == ObjectVariantEnum.Int || right.Variant == ObjectVariantEnum.Int)
                    {
                        return new OV_integer(this.@operator(left.Content, right.Content));
                    }

                    // todo
                }
            }

            throw new NotImplementedException();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(this._symbol);
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.l.WriteXml(writer);
            this.r.WriteXml(writer);
            writer.WriteEndElement();
        }
    }

    class SparqlDivideExpression: SparqlBinaryExpression
    {
        public SparqlDivideExpression(SparqlExpression l, SparqlExpression r) : base(l, r, (o, o1) =>
        {
            if (o is int && o1 is int)
                return Convert.ToDecimal(o)/o1;
            return o/o1;
        }, "divide")
        {
        }

        protected override ObjectVariants ApplyTyped(ObjectVariants left, ObjectVariants right)
        {
            if(left.Variant==ObjectVariantEnum.Int && right.Variant==ObjectVariantEnum.Int)
                return new OV_decimal(Convert.ToDecimal(left.Content) / (int)right.Content);
            return base.ApplyTyped(left, right);
        }
    }

    public class SparqlBinaryExpression<T> : SparqlExpression
    {
        private readonly SparqlExpression _l;
        private readonly SparqlExpression _r;
        private readonly string _symbol;

        public SparqlBinaryExpression(SparqlExpression l, SparqlExpression r, Func<dynamic, dynamic, dynamic> @operator, Func<dynamic, T> typedCtor, string symbol)
        {
            this._l = l;
            this._r = r;
            this._symbol = symbol;
            var lc = l.Const;
            var rc = r.Const;

            switch (NullablePairExt.Get(lc, rc))
            {
                case NP.bothNull:
                    this.Operator = result => @operator(l.Operator(result), r.Operator(result));
                    this.AggregateLevel = SetAggregateLevel(l.AggregateLevel, r.AggregateLevel);
                    break;
                case NP.leftNull:
                    this.Operator = result => @operator(l.Operator(result), rc.Content);
                    this.AggregateLevel = l.AggregateLevel;
                    break;
                case NP.rigthNull:
                    this.Operator = result => @operator(lc.Content, r.Operator(result));
                    this.AggregateLevel = r.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    this.Const = typedCtor(@operator(lc.Content, rc.Content));
                    break;
            }

            this.TypedOperator = res => typedCtor(this.Operator(res));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(this._symbol);
            writer.WriteAttributeString("type", this.GetType().ToString());
            this._l.WriteXml(writer);
            this._r.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}