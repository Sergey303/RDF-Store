using RDFCommon;
using RDFCommon.Interfaces;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;

namespace SparqlQuery.SparqlClasses.Expressions
{
    using System.Xml;

    class SparqlVarExpression : SparqlExpression
    {
       public VariableNode Variable;

        public SparqlVarExpression(VariableNode variableNode) :base(VariableDependenceGroupLevel.SimpleVariable, false)
        {
            // TODO: Complete member initialization
            this.Variable = variableNode;
            this.Operator = result =>
            {
                // ObjectVariants sparqlVariableBinding;
                ////if (result.TryGetValue(Variable, out sparqlVariableBinding))
                ////    return sparqlVariableBinding;
                ////else return new SparqlUnDefinedNode();
                var value = result[variableNode];
                if(value is IIriNode)
                    return value;
                else return value.Content;
            };
            this.TypedOperator = result =>
            {
                // ObjectVariants sparqlVariableBinding;
                ////if (result.TryGetValue(Variable, out sparqlVariableBinding))
                ////    return sparqlVariableBinding;
                ////else return new SparqlUnDefinedNode();
                return result[variableNode];
            };
        }

        public override void WriteXml(XmlWriter writer)
        {
            this.Variable.WriteXml(writer);
        }





    }
}
