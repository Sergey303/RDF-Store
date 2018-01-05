using System;
using System.Xml;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlUnaryExpression : SparqlExpression
    {
        private readonly SparqlExpression _child;
        private readonly string _symbol;


        public SparqlUnaryExpression(Func<dynamic, dynamic> @operator, SparqlExpression child, string symbol)
            : base(child.AggregateLevel, child.IsStoreUsed)
        {
            this._child = child;
            this._symbol = symbol;
            var childConst = child.Const;

            if (childConst != null) this.Const = childConst.Change(@operator(childConst.Content));
            else
            {
                this.Operator = result => @operator(child.Operator(result));
                this.TypedOperator = results => child.TypedOperator(results).Change(@operator);
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(this._symbol);
            writer.WriteAttributeString("type", this.GetType().ToString());
            this._child.WriteXml(writer);
            writer.WriteEndElement();
        }
    }

    internal class SparqlUnaryExpression<T> : SparqlExpression where T : ObjectVariants
    {
        public SparqlUnaryExpression(Func<dynamic, dynamic> @operator, SparqlExpression child,
            Func<dynamic, ObjectVariants> ctor)
            : base(child.AggregateLevel, child.IsStoreUsed)
        {
            this.Operator = @operator;
         
            var childConst = child.Const;
            if (childConst != null) this.Const = ctor(@operator(childConst.Content));
            else
            {
                this.Operator = result => @operator(child.Operator(result));
                this.TypedOperator = results => ctor(this.Operator(results).Change(@operator));
            }
        }
    }
}
