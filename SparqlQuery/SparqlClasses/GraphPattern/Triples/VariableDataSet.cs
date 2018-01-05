using System.Collections.Generic;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples
{
    using System.Xml;

    using SparqlQuery.SparqlClasses.Query;

    public class VariableDataSet : DataSet
    {
        public VariableNode Variable;

        public VariableDataSet()
        {
        }

        public VariableDataSet(VariableNode variable, IEnumerable<ObjectVariants> @base):base(@base)
            
        {
            this.Variable = variable;
        }

        // public IEnumerable<IUriNode> GetGraphUri(SparqlResult variablesBindings)
        // {
        // SparqlVariableBinding fixedGraph;
        // if (!variablesBindings.row.TryGetValue(Variable, out fixedGraph)) return this;
        // var uriNode = fixedGraph.Value as IUriNode;
        // if (uriNode == null) throw new ArgumentOutOfRangeException("graphs variable's value");
        // return Enumerable.Repeat(uriNode,1);
        // }
        public override void ReadXml(XmlReader reader)
        {
            this.Variable = (VariableNode)SparqlQuery.CreateByTypeAttribute(reader);
            base.ReadXml(reader);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("varGraphs");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.Variable.WriteXml(writer);
            foreach (var graph in this) graph.WriteXml(writer);

            writer.WriteEndElement();
        }
    }
}