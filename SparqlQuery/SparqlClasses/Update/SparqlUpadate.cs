using System.Xml;
using System.Xml.Schema;
using RDFCommon;
using RDFCommon.Interfaces;

namespace SparqlQuery.SparqlClasses.Update
{

    public abstract class SparqlUpdateSilent : ISparqlUpdate
    {
        protected bool IsSilent;

        internal void Silent()
        {
            this.IsSilent = true;
        }

        public abstract void RunUnSilent(IStore store);

        public void Run(IStore store)
        {
            if (this.IsSilent)
                try
                {
                    this.RunUnSilent(store);
                }
                catch
                {
                    // TODO
                }
            else this.RunUnSilent(store);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            IsSilent = bool.Parse(reader.GetAttribute("silent"));
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("silent", this.IsSilent.ToString());
        }
    }
}

