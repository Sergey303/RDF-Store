using System;
using System.Collections.Generic;
using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using SparqlQuery.SparqlClasses.Expressions;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SolutionModifier
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    public class SparqlOrderCondition :IXmlSerializable
    {
        private readonly Func<dynamic, dynamic> orderCondition = node =>
        {
            if (node is SparqlUnDefinedNode) return string.Empty;
            if (node is IBlankNode) return node.ToString();
            if (node is ILiteralNode) return node.ToString();
            if (node is IIriNode) return node.ToString();
            return node;
        };
        private readonly Func<SparqlResult, dynamic> getNode;

        private readonly Func<dynamic, int> orderByTypeCondition = node =>
        {
            if (node is SparqlUnDefinedNode)
                return 0;
            if (node is IBlankNode)
                return 1;
            if (node is IIriNode )
                return 2;
            if (node is IStringLiteralNode)
                return 3;
            return 4;
        };
        private SparqlOrderDirection direction=SparqlOrderDirection.Asc;
        private SparqlExpression.VariableDependenceGroupLevel AggregateLevel;
        private RdfQuery11Translator q;

        private SparqlExpression sparqlExpression;

        private SparqlFunctionCall sparqlFunctionCall;

        private VariableNode variableNode;

        public SparqlOrderCondition(SparqlExpression sparqlExpression, string dir, RdfQuery11Translator q)
        {
            this.q = q;
            this.sparqlExpression = sparqlExpression;

            // TODO: Complete member initialization
            switch (dir.ToLower())
            {
                case "desc":
                    this.direction = SparqlOrderDirection.Desc;
                    break;
                case "asc":
                default:
                    this.direction = SparqlOrderDirection.Asc;
                    break;
            }

            this.getNode = sparqlExpression.Operator;
            this.AggregateLevel = sparqlExpression.AggregateLevel;
        }

        private enum SparqlOrderDirection
        {
            Desc,
            Asc
        }

        public SparqlOrderCondition(SparqlExpression sparqlExpression, RdfQuery11Translator q)
        {
            this.sparqlExpression = sparqlExpression;
            this.q = q;

            // TODO: Complete member initialization
            this.getNode = sparqlExpression.Operator;
            this.AggregateLevel = sparqlExpression.AggregateLevel;
        }

        public SparqlOrderCondition(SparqlFunctionCall sparqlFunctionCall, RdfQuery11Translator q)
        {
            this.sparqlFunctionCall = sparqlFunctionCall;
            this.q = q;

            // TODO: Complete member initialization
            this.getNode = sparqlFunctionCall.Operator;
            this.AggregateLevel = sparqlFunctionCall.AggregateLevel;
        }

        public SparqlOrderCondition(VariableNode variableNode, RdfQuery11Translator q)
        {
            this.variableNode = variableNode;
            this.q = q;

            // TODO: Complete member initialization
            this.getNode = result => result[variableNode] ?? new SparqlUnDefinedNode();
            this.AggregateLevel = SparqlExpression.VariableDependenceGroupLevel.SimpleVariable;
        }

        public IEnumerable<SparqlResult> Order(IEnumerable<SparqlResult> resultSet)
       {
            if (this.AggregateLevel == SparqlExpression.VariableDependenceGroupLevel.Const)
                return resultSet;
            SparqlResult[] toOrderArray = resultSet as SparqlResult[] ??
                                          resultSet.Select(result => result.Clone()).ToArray();
            if (toOrderArray.Length < 2) return toOrderArray;
                if (this.AggregateLevel == SparqlExpression.VariableDependenceGroupLevel.Group)
                {
                   // if (toOrderArray is SparqlGroupsCollection) return toOrderArray.Cast<SparqlGroupOfResults>().Select(g => { g.Group = Order(g.Group); return g;});
                   // else
                        toOrderArray = new SparqlResult[] {new SparqlGroupOfResults(this.q) {Group = toOrderArray}};
                }
                else if (this.AggregateLevel == SparqlExpression.VariableDependenceGroupLevel.GroupOfGroups)
                {
                    if (!(toOrderArray[0] is SparqlGroupOfResults)) throw new Exception("requested grouping");
                    toOrderArray = new SparqlResult[] {new SparqlGroupOfResults(this.q) {Group = toOrderArray}};
                }

            switch (this.direction)
           {
               case SparqlOrderDirection.Desc:
                   return from r in toOrderArray
                              let node= this.getNode(r)
                              orderby this.orderByTypeCondition(node) descending, this.orderCondition(node) descending 
                              select r;
                   break;
               case SparqlOrderDirection.Asc:
               default:
                   return from r in toOrderArray
                          let node = this.getNode(r)
                          orderby this.orderByTypeCondition(node), this.orderCondition(node)
                          select r;
                   break;
           }
        }

        public IEnumerable<SparqlResult> Order4Grouped(IEnumerable<SparqlResult> resultSet)
        {
            if (this.AggregateLevel == SparqlExpression.VariableDependenceGroupLevel.Const)
                return resultSet;
            var toOrderArray = resultSet.Select(result => result.Clone());
                if (this.AggregateLevel == SparqlExpression.VariableDependenceGroupLevel.GroupOfGroups)
                    toOrderArray = new SparqlResult[] {new SparqlGroupOfResults(this.q) {Group = toOrderArray}};
            switch (this.direction)
            {
                case SparqlOrderDirection.Desc:
                    return from r in toOrderArray
                           let node = this.getNode(r)
                           orderby this.orderByTypeCondition(node) descending, this.orderCondition(node) descending
                           select r;
                    break;
                case SparqlOrderDirection.Asc:
                default:
                    return from r in toOrderArray
                           let node = this.getNode(r)
                           orderby this.orderByTypeCondition(node), this.orderCondition(node)
                           select r;
                    break;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Enum.TryParse(reader.GetAttribute("Group"), out this.AggregateLevel);
            var readed = Query.SparqlQuery.CreateByTypeAttribute(reader);
            if (readed == null) throw new Exception("readed null");
            if ((this.sparqlFunctionCall = readed as SparqlFunctionCall) == null)
            {
                if ((this.sparqlExpression = readed as SparqlExpression) == null)
                    if ((this.variableNode = readed as VariableNode) == null) throw new Exception("readed null");
            }
            else
            {
                var readed2 = Query.SparqlQuery.CreateByTypeAttribute(reader);
                if (readed2 == null) return;
                if ((this.sparqlExpression = readed2 as SparqlExpression) == null)
                {
                    if ((this.variableNode = readed2 as VariableNode) == null) throw new Exception("readed some else");
                }
                else
                {
                    var readed3 = Query.SparqlQuery.CreateByTypeAttribute(reader);
                    if (readed3 == null) return;

                    if ((this.variableNode = readed3 as VariableNode) == null) throw new Exception("readed some else");
                }
            }

        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("order");
            writer.WriteAttributeString("type", this.GetType().ToString());
            writer.WriteAttributeString("GroupAggregateLevel", this.AggregateLevel.ToString());

            this.sparqlFunctionCall?.WriteXml(writer);

            this.sparqlExpression?.WriteXml(writer);
            this.variableNode?.WriteXml(writer);

            writer.WriteEndElement();
        }
    }
}
