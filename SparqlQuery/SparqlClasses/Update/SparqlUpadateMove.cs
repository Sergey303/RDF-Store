using System;
using RDFCommon;
using RDFCommon.Interfaces;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;

    public class SparqlUpdateMove : SparqlUpdateSilent
    {
        public string To;
        public string From;     
        public override void RunUnSilent(IStore store)
        {
            if ((this.From == null && this.To == null) || (this.From != null && this.From.Equals(this.To))) return;
            IGraph fromGraph;
            if (this.From == null) fromGraph = store;
            else
            {
                fromGraph = store.NamedGraphs.GetGraph(this.From);
                if (!fromGraph.Any()) throw new Exception(this.From);
            }

            if (this.To == null)
            {
                fromGraph.GetTriples(
                    (s, p, o) =>
                        {
                            store.Add(s, p, o);
                            return true;
                        });

                fromGraph.Clear();
            }
            else // if (!store.NamedGraphs.ContainsGraph(To)) 
                store.NamedGraphs.AddGraph(this.To, fromGraph);

            // else store.NamedGraphs.ReplaceGraph(To,fromGraph);
        }
        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            reader.ReadStartElement("from");
            this.From=reader.ReadString();
            reader.ReadEndElement();

            reader.ReadStartElement("to");
            this.To=reader.ReadString();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("move");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
            writer.WriteStartElement("from");
            writer.WriteString(this.From);
            writer.WriteEndElement();

            writer.WriteStartElement("to");
            writer.WriteString(this.To);
            writer.WriteEndElement();
        }
    }
}
