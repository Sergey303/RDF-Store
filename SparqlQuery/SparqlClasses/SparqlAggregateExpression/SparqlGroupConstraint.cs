using System;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.Expressions;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SparqlAggregateExpression
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    public class SparqlGroupConstraint : IXmlSerializable
    {
        public readonly Func<SparqlResult, ObjectVariants> Constrained;

        private SparqlExpression sparqlExpression;

        private SparqlFunctionCall sparqlFunctionCall;

        private VariableNode variableNode;

        public SparqlGroupConstraint(SparqlExpression sparqlExpression)
        {
            this.sparqlExpression = sparqlExpression;
            //if(sparqlExpression.Const!=null)
                // TODO: Const

                this.Constrained = sparqlExpression.TypedOperator;
        }

        public SparqlGroupConstraint(SparqlFunctionCall sparqlFunctionCall)
        {
            // TODO: Complete member initialization
            this.Constrained = sparqlFunctionCall.TypedOperator;
            this.sparqlFunctionCall = sparqlFunctionCall;

        }

        public SparqlGroupConstraint(VariableNode variableNode)
        {
            this.variableNode = variableNode;
            this.Constrained = result => result[variableNode];
        }

        public VariableNode Variable { get; set; }

        public SparqlGroupConstraint(SparqlExpressionAsVariable sparqlExpressionAsVariable)
        {
            // TODO: Complete member initialization
            this.Variable = sparqlExpressionAsVariable.variableNode;
            this.Constrained = sparqlExpressionAsVariable.RunExpressionCreateBind;
 
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var field = SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader);
            sparqlExpression = field as SparqlExpression;
            sparqlFunctionCall=field as SparqlFunctionCall;
            variableNode = field as VariableNode;
        }

        public void WriteXml(XmlWriter writer)
        {
        writer.WriteStartElement("groupConstrain");
            writer.WriteAttributeString("type", this.GetType().ToString());
            switch (NullableTripleExt.Get(this.sparqlExpression, this.sparqlFunctionCall, this.variableNode))
            {
              
                case NT.FirstNotNull:
                    this.sparqlExpression.WriteXml(writer);
                    break;
                case NT.SecondNotNull:
                    this.sparqlFunctionCall.WriteXml(writer);
                    break;
                case NT.ThirdNotNull:
                    this.WriteXml(writer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            writer.WriteEndElement();
        }
    }
}
