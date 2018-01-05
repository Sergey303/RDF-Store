using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RDFCommon.OVns;
using RDFCommon.OVns.numeric;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SparqlAggregateExpression
{

    class SparqlAvgExpression : SparqlAggregateExpression
    {
        protected override void Create()
        {
            this.Const = this.Expression.Const;
            if (this.IsDistinct)
            {
                this.Operator = result =>
                    {
                        if (result is SparqlGroupOfResults)
                        {
                            var @group = (result as SparqlGroupOfResults).Group.ToArray();

                            if (group.Length == 0) return 0;
                            if (group.Length == 1) return this.Expression.Operator(group[0]);
                            return @group.Select(this.Expression.Operator).Distinct().Average(o1 => o1);
                        }
                        else throw new Exception();
                    };
                this.TypedOperator = result =>
                    {
                        if (result is SparqlGroupOfResults)
                        {
                            var @group = (result as SparqlGroupOfResults).Group.ToArray();

                            // .Select(sparqlResult => sparqlResult.Clone())
                            if (group.Length == 0) return new OV_int(0);
                            if (group.Length == 1) return this.Expression.TypedOperator(group[0]);
                            var firstTyped = this.Expression.TypedOperator(@group[0]);
                            var list = new List<dynamic>() { firstTyped.Content };
                            list.AddRange(@group.Skip(1).Select(this.Expression.Operator));

                            return firstTyped.Change(o => list.Distinct().Average(o1 => o1));
                        }
                        else throw new Exception();
                    };
            }
            else
            {
                this.Operator = result =>
                    {
                        if (result is SparqlGroupOfResults)
                        {
                            var @group = (result as SparqlGroupOfResults).Group.ToArray();

                            if (group.Length == 0) return 0;
                            if (group.Length == 1) return this.Expression.Operator(group[0]);

                            return @group.Select(this.Expression.Operator).Average(o1 => o1);
                        }
                        else throw new Exception();
                    };
                this.TypedOperator = result =>
                    {
                        if (!(result is SparqlGroupOfResults)) throw new Exception();
                        var @group = (result as SparqlGroupOfResults).Group.ToArray();

                        // .Select(sparqlResult => sparqlResult.Clone())
                        if (@group.Length == 0) return new OV_int(0);
                        if (@group.Length == 1) return this.Expression.TypedOperator(@group[0]);
                        var firstTyped = this.Expression.TypedOperator(@group[0]);
                        var list = new List<dynamic>() { firstTyped.Content };
                        list.AddRange(@group.Skip(1).Select(this.Expression.Operator));
                        return firstTyped.Change(o => list.Average(o1 => o1));
                    };
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("avg");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
        }

     
    }
}

