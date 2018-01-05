using System;
using RDFCommon;
using RDFCommon.Interfaces;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;

    public class SparqlUpdateAdd : SparqlUpdateSilent
    {
        public string To;
        public string From;          

        public override void RunUnSilent(IStore store)
        {
            if ((this.From == null && this.To == null) || this.From != null && this.From.Equals(this.To)) return;
            IGraph fromGraph, toGraph;
            if (this.From == null) fromGraph = store;
            else
            {
                fromGraph = store.NamedGraphs.GetGraph(this.From);
                if (!fromGraph.Any()) throw new Exception(this.From);
            }

            if (this.To == null) toGraph = store;
            else
                toGraph = store.NamedGraphs.GetGraph(this.To);// ?? store.NamedGraphs.CreateGraph(To);

            foreach (var t in fromGraph.GetTriples((s, p, o) =>
            {
                toGraph.Add(s, p, o);
                return true;
            }))
            {
                
            } 


        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            reader.ReadStartElement("from");
            From = reader.ReadContentAsString();
            reader.ReadEndElement();

            reader.ReadStartElement("to");
            To = reader.ReadContentAsString();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("add");
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
