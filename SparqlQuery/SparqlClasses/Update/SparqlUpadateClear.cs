using System;
using RDFCommon;
using RDFCommon.Interfaces;
using SparqlQuery.SparqlClasses.GraphPattern;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml;

    public class SparqlUpdateClear : SparqlUpdateSilent 
    {
       public UpdateGraph   Graph;

        public override void RunUnSilent(IStore store)
        {
            switch (this.Graph.SparqlGrpahRefTypeEnum)
            {
                case SparqlGrpahRefTypeEnum.Setted:
                    store.NamedGraphs.Clear(this.Graph.Name);
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
            writer.WriteStartElement("clear");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
            Graph.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
