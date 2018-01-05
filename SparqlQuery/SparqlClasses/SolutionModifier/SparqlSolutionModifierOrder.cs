using System.Collections.Generic;
using System.Linq;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SolutionModifier
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public class SparqlSolutionModifierOrder  :List<SparqlOrderCondition>, IXmlSerializable
    {
        private readonly string name="order";

        public IEnumerable<SparqlResult> Order(IEnumerable<SparqlResult> enumerable)
        {
           return this.Aggregate(enumerable, (current, order) => order.Order(current));
        }

        public IEnumerable<SparqlResult> Order4Grouped(IEnumerable<SparqlResult> enumerable)
        {
           return this.Aggregate(enumerable, (current, order) => order.Order4Grouped(current));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(XmlReader reader)
        {

            while (reader.IsStartElement())
            {
                this.Add((SparqlOrderCondition)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader));

            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(this.name);
            writer.WriteAttributeString("type", this.GetType().ToString());
            foreach (var expr in this)
            {
                expr.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
    }
}
