using System;
using System.Linq;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SparqlAggregateExpression
{
    using System.Xml;

    class SparqlCountExpression : SparqlAggregateExpression
    {
        public SparqlCountExpression() :base()
        {
            this.Const=null;
            if (this.IsDistinct)
            {
                this.Operator = result =>
                {
                    var groupOfResults = result as SparqlGroupOfResults;
                    if (groupOfResults != null)
                        return new OV_int(groupOfResults.Group.Count());
                    else throw new Exception();
                };
            }
            else
            {
                this.Operator = result =>
                {
                    var groupOfResults = result as SparqlGroupOfResults;
                    if (groupOfResults != null)
                        return new OV_int(groupOfResults.Group.Count());
                    else throw new Exception();
                };                                                 
            }

            this.TypedOperator = result => new OV_int(this.Operator(result));
        }

        protected override void Create()
        {
          
        }
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Count");
            writer.WriteAttributeString("type", this.GetType().ToString());
            
            base.WriteXml(writer);
        }
    }
}
