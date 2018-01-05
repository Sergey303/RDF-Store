using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.Expressions;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    public class SparqlExpressionAsVariable : VariableNode, ISparqlGraphPattern
    {
        public VariableNode variableNode;
        public SparqlExpression sparqlExpression;
        private readonly RdfQuery11Translator q;

        public SparqlExpressionAsVariable(VariableNode variableNode, SparqlExpression sparqlExpression, RdfQuery11Translator q) : base(variableNode.VariableName, variableNode.Index)
        {
            // TODO: Complete member initialization
            this.variableNode = variableNode;
            this.sparqlExpression = sparqlExpression;
            this.q = q;
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            switch (this.sparqlExpression.AggregateLevel)
            {
                case SparqlExpression.VariableDependenceGroupLevel.Const:
                    return variableBindings.Select(
                        variableBinding =>
                        {
                            variableBinding.Add(this.variableNode, this.sparqlExpression.Const);
                            return variableBinding;
                        });
                    break;
                case SparqlExpression.VariableDependenceGroupLevel.UndependableFunc:
                case SparqlExpression.VariableDependenceGroupLevel.SimpleVariable:
                    return variableBindings.Select(
                        variableBinding =>
                        {
                            variableBinding.Add(this.variableNode, this.sparqlExpression.TypedOperator(variableBinding));
                            return variableBinding;
                        });
                case SparqlExpression.VariableDependenceGroupLevel.Group:
                    this.sparqlExpression.TypedOperator(new SparqlGroupOfResults(this.q) {Group = variableBindings});
                    break;
                case SparqlExpression.VariableDependenceGroupLevel.GroupOfGroups:
                    throw new Exception("groping requested");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return variableBindings.Select(
                variableBinding =>
                {
                    variableBinding.Add(this.variableNode, this.RunExpressionCreateBind(variableBinding));
                    return variableBinding;
                });
        }

        public IEnumerable<SparqlResult> Run4Grouped(IEnumerable<SparqlResult> variableBindings)
        {
            switch (this.sparqlExpression.AggregateLevel)
            {
                case SparqlExpression.VariableDependenceGroupLevel.Const:
                    return variableBindings.Select(
                        variableBinding =>
                        {
                            variableBinding.Add(this.variableNode, this.sparqlExpression.Const);
                            return variableBinding;
                        });
                    break;
                case SparqlExpression.VariableDependenceGroupLevel.UndependableFunc:
                case SparqlExpression.VariableDependenceGroupLevel.SimpleVariable:
                 
                case SparqlExpression.VariableDependenceGroupLevel.Group:
                    return this.RunAddVar(variableBindings);
                    break;
                case SparqlExpression.VariableDependenceGroupLevel.GroupOfGroups:
                    var arr = variableBindings as SparqlResult[] ?? variableBindings.Select(result => result.Clone()).ToArray();
                    var groupOfGroups = new SparqlGroupOfResults(this.q) {Group = arr};
                    return arr.Select(variableBinding =>
                    {
                        variableBinding.Add(this.variableNode, this.sparqlExpression.TypedOperator(groupOfGroups));
                        return variableBinding;
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
           
        }

        public IEnumerable<SparqlResult> RunAddVar(IEnumerable<SparqlResult> variableBindings)
        {
            return variableBindings.Select(
                variableBinding =>
                {
                    variableBinding.Add(this.variableNode, this.sparqlExpression.TypedOperator(variableBinding));
                    return variableBinding;
                });
        }

        public SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.Bind;} }

        /// <summary>
        /// The run expression create bind.
        /// </summary>
        /// <param name="variableBinding">
        /// The variable binding.
        /// </param>
        /// <returns>
        /// The <see cref="ObjectVariants"/>.
        /// </returns>
        public ObjectVariants RunExpressionCreateBind(SparqlResult variableBinding)
        {
             return this.sparqlExpression.TypedOperator(variableBinding);
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("varAsExpression");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.variableNode.WriteXml(writer);
            this.sparqlExpression.WriteXml(writer);
            writer.WriteEndElement();
        }
    }

}
