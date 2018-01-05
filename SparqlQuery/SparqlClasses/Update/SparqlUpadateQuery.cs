using System;
using System.Collections.Generic;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;

    public class SparqlUpdateQuery : Query.SparqlQuery
    {
        public List<ISparqlUpdate> Updates = new List<ISparqlUpdate>();

        public SparqlUpdateQuery(RdfQuery11Translator q) : base(q)
        {
            this.ResultSet.ResultType = ResultType.Update;
        }

       

        public override SparqlResultSet Run()
        {
            try
            {
                foreach (var sparqlUpdate in this.Updates)
                    sparqlUpdate.Run(this.q.Store);
                this.ResultSet.UpdateStatus = SparqlUpdateStatus.ok;
                this.ResultSet.UpdateMessage ="ok";
                return this.ResultSet;
            }
            catch (Exception e)
            {
                this.ResultSet.UpdateStatus = SparqlUpdateStatus.fail;
                this.ResultSet.UpdateMessage = e.Message;
            }

            return this.ResultSet;
        }

        internal void Create(ISparqlUpdate sparqlUpdate)
        {
            this.Updates.Add(sparqlUpdate);
        }

        internal void Add(SparqlUpdateQuery sparqlUpdateQuery)
        {
            this.Updates.AddRange(sparqlUpdateQuery.Updates);
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            Updates=new List<ISparqlUpdate>();
            while (reader.IsStartElement())
            {
                Updates.Add((ISparqlUpdate) Query.SparqlQuery.CreateByTypeAttribute(reader));
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("update");
            writer.WriteAttributeString("type", this.GetType().ToString());
            //base.WriteXml(writer);
            foreach (var update in this.Updates)
            {
                update.WriteXml(writer);
            }
            writer.WriteEndElement();
        }

        // public override SparqlQueryTypeEnum QueryType
        // {
        // get{ return SparqlQueryTypeEnum.Update;}
        // }
    }
}
