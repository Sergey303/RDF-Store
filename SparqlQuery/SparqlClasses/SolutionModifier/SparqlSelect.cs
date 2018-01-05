using System;
using System.Collections.Generic;
using System.Linq;
using SparqlQuery.SparqlClasses.Expressions;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SolutionModifier
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable]
    public class SparqlSelect : HashSet<VariableNode>, IXmlSerializable
    {

        private bool isAll;

        internal bool IsReduced;

        private RdfQuery11Translator q;

        public List<VariableNode> Selected;

        /// <summary>
        /// 4 Deserialization only
        /// </summary>
        public SparqlSelect()
        {
            
        }
        public SparqlSelect(RdfQuery11Translator q)
        {
            this.q = q;
        }


        internal bool IsDistinct { get; set; }


        internal void IsAll()
        {
            this.isAll = true;
        }


        public IEnumerable<SparqlResult> Run(
            IEnumerable<SparqlResult> variableBindings,
            SparqlResultSet resultSet,
            bool isGrouped)
        {
            this.Selected = null;

            if (this.isAll)
            {
                this.Selected = resultSet.Variables.Values.Where(v => !(v is SparqlBlankNode)).ToList();

            }
            else
            {
                var asExpressions = this.Select(varOrExpr => varOrExpr as SparqlExpressionAsVariable).ToArray();
                if (isGrouped)
                {
                    if (asExpressions.All(exp => exp != null))
                    {
                        if (
                            asExpressions.All(
                                exp =>
                                    exp.sparqlExpression.AggregateLevel
                                    == SparqlExpression.VariableDependenceGroupLevel.GroupOfGroups)) return this.OneRowResult(variableBindings, asExpressions);
                    }
                    else
                    {
                        // todo
                    }
                }
                else
                {
                    if (asExpressions.All(exp => exp != null))
                    {
                        // if(asExpressions.All(exp=>exp.sparqlExpression.AggregateLevel==SparqlExpression.VariableDependenceGroupLevel.Const || exp.sparqlExpression.AggregateLevel==SparqlExpression.VariableDependenceGroupLevel.UndependableFunc))
                        if (
                            asExpressions.All(
                                exp =>
                                    exp.sparqlExpression.AggregateLevel
                                    == SparqlExpression.VariableDependenceGroupLevel.Group)) return this.OneRowResult(variableBindings, asExpressions);
                    }
                }

                this.Selected = new List<VariableNode>();

                foreach (VariableNode variable in this)
                {
                    var expr = variable as SparqlExpressionAsVariable;
                    if (expr != null)
                    {
                        variableBindings = isGrouped ? expr.Run4Grouped(variableBindings) : expr.Run(variableBindings);
                        this.Selected.Add(expr.variableNode);
                    }
                    else this.Selected.Add(variable);
                }
            }

            variableBindings = variableBindings.Select(
                result =>
                    {
                        result.SetSelection(this.Selected);
                        return result;
                    });
            if (this.IsDistinct) variableBindings = Distinct(variableBindings);
            if (this.IsReduced) variableBindings = Reduce(variableBindings);

            return variableBindings;
        }

        private IEnumerable<SparqlResult> OneRowResult(
            IEnumerable<SparqlResult> variableBindings,
            SparqlExpressionAsVariable[] asExpressions)
        {
            var oneRowResult = new SparqlResult(this.q);
            oneRowResult.SetSelection(asExpressions.Select(exprVar => exprVar.variableNode));
            foreach (var sparqlExpressionAsVariable in asExpressions)
                oneRowResult.Add(
                    sparqlExpressionAsVariable.RunExpressionCreateBind(
                        new SparqlGroupOfResults(this.q) { Group = variableBindings }),
                    sparqlExpressionAsVariable.variableNode);
            return Enumerable.Range(0, 1).Select(i => oneRowResult);
        }

        private static IEnumerable<SparqlResult> Reduce(IEnumerable<SparqlResult> results)
        {
            var duplicated = new Dictionary<SparqlResult, bool>();
            foreach (var res in results)
            {
                if (duplicated.ContainsKey(res))
                {
                    if (duplicated[res]) continue;
                    duplicated[res] = true;
                }
                else duplicated.Add(res, false);
                yield return res;
            }
        }

        private static IEnumerable<SparqlResult> Distinct(IEnumerable<SparqlResult> results)
        {
            var history = new HashSet<SparqlResult>();
            foreach (var res in results.Where(res => !history.Contains(res)))
            {
                history.Add(res);
                yield return res;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
          
            this.isAll = bool.Parse(reader.GetAttribute("all"));
            this.IsReduced = bool.Parse(reader.GetAttribute("reduce"));
            this.IsDistinct = bool.Parse(reader.GetAttribute("distinct"));

            if (!isAll)
                while (reader.IsStartElement())
            {
                Add((VariableNode)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader));
            }
            
            
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("select");
            writer.WriteAttributeString("type", this.GetType().ToString());
            writer.WriteAttributeString("all", this.isAll.ToString());
            writer.WriteAttributeString("reduce", this.IsReduced.ToString());
            writer.WriteAttributeString("distinct", this.IsDistinct.ToString());
            if (!isAll)
            {
                foreach (var variableNode in this)
                {
                    variableNode.WriteXml(writer);
                }
             }

            writer.WriteEndElement();
        }
    }
}

