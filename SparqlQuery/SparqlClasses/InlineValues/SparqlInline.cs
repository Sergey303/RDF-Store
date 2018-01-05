using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.InlineValues
{
    public class SparqlInline : ISparqlGraphPattern
    {
        private readonly List<VariableNode> variables = new List<VariableNode>();
        private int currentVariableIndex=0; 
        public List<SparqlVariableBinding[]> VariablesBindingsList=new List<SparqlVariableBinding[]>();
        internal void AddVar(VariableNode variableNode)
        {
            this.variables.Add(variableNode);
        }

        internal void AddValue(ObjectVariants sparqlNode)
        {
              if (this.currentVariableIndex == 0) this.VariablesBindingsList.Add(new SparqlVariableBinding[this.variables.Count]);
            if (sparqlNode is SparqlUnDefinedNode) {
                this.currentVariableIndex++; return; }

            this.VariablesBindingsList.Last()[this.currentVariableIndex] = new SparqlVariableBinding(this.variables[this.currentVariableIndex], sparqlNode);
            this.currentVariableIndex++;     
        }

        internal void NextListOfVarBindings()
        {
            this.currentVariableIndex = 0;
          
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> bindings)
        {
            ObjectVariants exists;
            foreach (SparqlResult result in bindings)
            {
                foreach (var arrayofBindings in this.VariablesBindingsList)
                {
                    bool iSContinue = false;
                    foreach (var sparqlVariableBinding in arrayofBindings.Where(binding => binding!=null))
                    {
                        exists = result[sparqlVariableBinding.Variable];
                        if (exists!=null)
                        {
                            if (exists.Equals(sparqlVariableBinding.Value)) continue;
                            iSContinue = true;
                            break;
                        }
                        else result.Add(sparqlVariableBinding.Variable, sparqlVariableBinding.Value);}

                    if (iSContinue) continue;
                    yield return result;
                }
            }
        }

        public SparqlGraphPatternType PatternType { get { return SparqlGraphPatternType.InlineDataValues; } }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {

            while (reader.IsStartElement("values"))
            {
                reader.ReadStartElement("   ");
               List<SparqlVariableBinding> bindings=new List<SparqlVariableBinding>();
                while (reader.IsStartElement("var"))
                {
                    VariableNode v = (VariableNode) Query.SparqlQuery.CreateByTypeAttribute(reader);
                    ObjectVariants value = (ObjectVariants) Query.SparqlQuery.CreateByTypeAttribute(reader);
                    bindings.Add(new SparqlVariableBinding(v, value));
                }
                VariablesBindingsList.Add(bindings.ToArray());
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("inline");
            writer.WriteAttributeString("type", this.GetType().ToString());
            foreach (var arrayBindings in this.VariablesBindingsList)
            {
                writer.WriteStartElement("values");
                foreach (SparqlVariableBinding binding in arrayBindings)
                {
                    binding.Variable.WriteXml(writer);
                    binding.Value.WriteXml(writer);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
