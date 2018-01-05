using System;
using System.Text;
using System.Xml.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;

    public class SparqlUpdateLoad : SparqlUpdateSilent
    {
        private ObjectVariants from;
        public string Graph;

        internal void SetIri(ObjectVariants sparqlUriNode)
        {
            this.from = sparqlUriNode;
        }

        internal void Into(string sparqlUriNode)
        {
            this.Graph = sparqlUriNode;
        }


        public override void RunUnSilent(IStore store)
        {
            using (LongWebClient wc = new LongWebClient() { })
            {
                // wc.Headers[HttpRequestHeader.ContentType] = "application/sparql-query"; //"query="+ 
                var gData = wc.DownloadData((string)this.@from.Content);
                string gString = Encoding.UTF8.GetString(gData);
                var graph = (this.Graph != null)
                    ? store.NamedGraphs.CreateGraph(this.Graph)
                    : store;
                try
                {
                    var gXml = XElement.Parse(gString);
                    graph.AddFromXml(gXml);
                }
                catch (Exception)
                {
                    try
                    {
                        graph.FromTurtle(gString);
                    }
                    catch (Exception)
                    {
                        //todo
                        throw;
                    }
                }
            }
        }
        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            Graph = reader.ReadContentAsString();
            @from=(ObjectVariants) Query.SparqlQuery.CreateByTypeAttribute(reader);

        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("load");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
           writer.WriteString(this.Graph);
            this.@from.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
