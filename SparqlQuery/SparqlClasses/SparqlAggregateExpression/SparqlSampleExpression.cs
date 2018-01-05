using System;
using System.Linq;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SparqlAggregateExpression
{
    using System.Xml;

    class SparqlSampleExpression : SparqlAggregateExpression
    {
        protected override void Create()
        {
            this.Const = this.Expression.Const;
            Random random=new Random();
            this.TypedOperator = result =>
            {
                    var spraqlGroupOfResults = result as SparqlGroupOfResults;
                if (spraqlGroupOfResults != null)
                    return this.Expression.TypedOperator(
                            spraqlGroupOfResults.Group.ElementAt(random.Next(spraqlGroupOfResults.Group.Count())));
                else throw new Exception();
            };

            this.Operator = result =>
                {                      
                    var spraqlGroupOfResults = result as SparqlGroupOfResults;
                    if (spraqlGroupOfResults != null)
                        return this.Expression.Operator(spraqlGroupOfResults.Group.ElementAt(random.Next(spraqlGroupOfResults.Group.Count())));
                    else throw new Exception();
                };
              
         
        }
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("sample");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
        }


    }
}
