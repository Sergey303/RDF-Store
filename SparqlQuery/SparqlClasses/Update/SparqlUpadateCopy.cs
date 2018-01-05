using RDFCommon;
using RDFCommon.Interfaces;
using SparqlQuery.SparqlClasses.GraphPattern;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;

    public class SparqlUpdateCopy : SparqlUpdateSilent
    {
        SparqlUpdateClear clear = new SparqlUpdateClear(){Graph = new UpdateGraph(SparqlGrpahRefTypeEnum.Default)};
        SparqlUpdateAdd add = new SparqlUpdateAdd();

        public override void RunUnSilent(IStore store)
        {
            this.clear.RunUnSilent(store);
            this.add.RunUnSilent(store);
        }

        public string To{
            set
            {
                this.clear.Graph.Name = value;
                this.clear.Graph.SparqlGrpahRefTypeEnum = SparqlGrpahRefTypeEnum.Setted;
                this.add.To = value;
            }
        }

        public string From           
        {
            set
            {
                this.add.From = value;
            }
        }
        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            add= (SparqlUpdateAdd) Query.SparqlQuery.CreateByTypeAttribute(reader);
            clear = (SparqlUpdateClear) Query.SparqlQuery.CreateByTypeAttribute(reader);

        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("copy");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
            this.add.WriteXml(writer);
            this.clear.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
