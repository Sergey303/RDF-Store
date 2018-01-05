using System.Collections.Generic;
using System.Linq;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Path
{
    using System.Xml;

    public class SparqlPathAlternative : SparqlPathTranslator
    {
        public readonly List<SparqlPathTranslator> alt = new List<SparqlPathTranslator>();

        public SparqlPathAlternative(SparqlPathTranslator p1, SparqlPathTranslator p2) : base(null)
        {
            this.alt = new List<SparqlPathTranslator>() {p1, p2};
        }                                                


        public override IEnumerable<ISparqlGraphPattern> CreateTriple(ObjectVariants subject, ObjectVariants @object, RdfQuery11Translator q)
        {
            var subjectNode = this.IsInverse ? @object : subject;
            var objectNode = this.IsInverse ? subject : @object;
            return this.alt.SelectMany(path => path.CreateTriple(subjectNode, objectNode, q));
        }

        internal override SparqlPathTranslator AddAlt(SparqlPathTranslator sparqlPathTranslator)
        {
            this.alt.Add(sparqlPathTranslator);
            return this;
        }

        public override void ReadXml(XmlReader reader)
        {
          
            this.alt .Clear();
            while (reader.IsStartElement())
            {
                this.alt.Add((SparqlPathTranslator)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader));
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("alternative");
            writer.WriteAttributeString("type", this.GetType().ToString());
            foreach (var triple in this.alt)
            {
                triple.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
    }
}