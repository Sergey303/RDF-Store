using System.Collections.Generic;
using System.Linq;
using SparqlQuery.SparqlClasses.Query.Result;
using SparqlQuery.SparqlClasses.SparqlAggregateExpression;

namespace SparqlQuery.SparqlClasses.SolutionModifier
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    public class SparqlSolutionModifierGroup : List<SparqlGroupConstraint>, IXmlSerializable
    {
        private readonly RdfQuery11Translator q;

        public SparqlSolutionModifierGroup(RdfQuery11Translator q)
        {
            this.q = q;
        }

        public IEnumerable<SparqlGroupOfResults> Group(IEnumerable<SparqlResult> enumerable)
        {
               if(this.Count==1)
                   if (this[0].Variable != null)
                       return enumerable.GroupBy(result => this[0].Constrained(result))
                           .Select(grouping =>
                               new SparqlGroupOfResults(this[0].Variable, grouping.Key, this.q) {Group = grouping});
                   else
                       return
                           enumerable.GroupBy(result => this[0].Constrained(result))
                               .Select(grouping => new SparqlGroupOfResults(this.q) {Group = grouping});


            return enumerable
                .GroupBy(result =>this.Select(constraint => constraint.Constrained(result)).ToList(), new CollectionEqualityComparer())
                .Select(grouping => new SparqlGroupOfResults(this.Select(constraint => constraint.Variable), grouping.Key, this.q) { Group = grouping });
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Clear();
            while (reader.IsStartElement("groupConstrain"))
            {
               Add((SparqlGroupConstraint) Query.SparqlQuery.CreateByTypeAttribute(reader));
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("group");
            writer.WriteAttributeString("type", this.GetType().ToString());
            foreach (var sgc in this)
            {
                sgc.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
    }
}
