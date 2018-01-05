using System;
using RDFCommon;
using RDFCommon.Interfaces;
using SparqlQuery.SparqlClasses.GraphPattern;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;

    public class SparqlUpdateDrop : SparqlUpdateSilent
    {
        public UpdateGraph Graph;
        public override void RunUnSilent(IStore store)
        {
            switch (this.Graph.SparqlGrpahRefTypeEnum)
            {
                case SparqlGrpahRefTypeEnum.Setted:
                    store.NamedGraphs.DropGraph(this.Graph.Name);
                    break;
                case SparqlGrpahRefTypeEnum.Default:
                    store.Clear();
                    break;
                case SparqlGrpahRefTypeEnum.Named:
                    store.NamedGraphs.ClearAllNamedGraphs();
                    break;
                case SparqlGrpahRefTypeEnum.All:
                    store.ClearAll();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            Graph = (UpdateGraph) Query.SparqlQuery.CreateByTypeAttribute(reader);

        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("drop");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
            this.Graph.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
