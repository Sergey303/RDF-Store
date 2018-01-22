using System;
using System.Xml;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Node
{
    public class VariableNode : ObjectVariants
    {
        public string VariableName;

        public int index;

        // public NodeType NodeType { get { return NodeType.Variable; } }
        // public INode Value;
        // public int index;
        /// <summary>
        /// 4 serialization only
        /// </summary>
        public VariableNode()
        {
        }

        public VariableNode(string variableName, int index)
        {
            this.VariableName = variableName;
            this.index = index;
        }

        public override object Content
        {
            get
            {
                return this.VariableName;
            }
        }

        public override ObjectVariants Change(Func<dynamic, dynamic> changing)
        {
            throw new NotImplementedException();
        }

        public override ObjectVariantEnum Variant
        {
            get
            {
                return ObjectVariantEnum.Index;
            }
        }

        public override object WritableValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            //reader.MoveToAttribute("name");
            //if (!reader.ReadAttributeValue()) { }
            this.VariableName = reader.GetAttribute("name");
          //  reader.MoveToAttribute("n");
           // reader.ReadAttributeValue();
            this.index = int.Parse(reader.GetAttribute("n"));
          //  reader.ReadAttributeValue();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("var");
            writer.WriteAttributeString("type", this.GetType().ToString());
            writer.WriteAttributeString("name",  this.VariableName);
            writer.WriteAttributeString("n",  this.index.ToString());
            writer.WriteEndElement();
        }
    }
}