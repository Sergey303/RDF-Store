using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.InlineValues
{
    public class SparqlInlineVariable : HashSet<ObjectVariants>, ISparqlGraphPattern
    {
        private VariableNode variableNode;

        public SparqlInlineVariable(VariableNode variableNode)
        {
            // TODO: Complete member initialization
            this.variableNode = variableNode;
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> bindings)
        {
            ObjectVariants exists;
            foreach (SparqlResult result in bindings)
            {
                exists = result[this.variableNode];
                if (exists != null)
                {
                    if (this.Contains(exists)) yield return result; // TODO test
                }
                else
                {
                   
                    foreach (var newvariableBinding in this)
                        yield return
                            result.Add(newvariableBinding, this.variableNode);
                    result[this.variableNode] = null;
                }
            }
        }

        public SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.InlineDataValues;} }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            variableNode = (VariableNode) Query.SparqlQuery.CreateByTypeAttribute(reader);
            Clear();
            while (reader.IsStartElement())
            {
                Add((ObjectVariants) Query.SparqlQuery.CreateByTypeAttribute(reader));
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("inlineOneVariable");
            writer.WriteAttributeString("type", this.GetType().ToString());
            variableNode.WriteXml(writer);
            foreach (var val in this)
            {
                val.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
    }
}
