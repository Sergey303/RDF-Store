using System.Collections.Generic;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Path
{
    using System.Xml;

    using SparqlQuery.SparqlClasses.Query;

    public class SparqlPathOneOrMany : SparqlPathTranslator
    {
        private SparqlPathTranslator path;

        public SparqlPathOneOrMany(SparqlPathTranslator path)
            : base(path.predicate)
        {
            this.path = path;
        }

        public override IEnumerable<ISparqlGraphPattern> CreateTriple(ObjectVariants  subject, ObjectVariants @object, RdfQuery11Translator q)
        {
            var subjectNode = this.IsInverse ? @object : subject;
            var objectNode = this.IsInverse ? subject : @object;
            yield return new SparqlPathManyTriple(subjectNode, this.path, objectNode, q);
        }

        public override void ReadXml(XmlReader reader)
        {
           this.path = (SparqlPathTranslator)SparqlQuery.CreateByTypeAttribute(reader);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("oneOrManyPath");

            writer.WriteAttributeString("type", this.GetType().ToString());
            this.path.WriteXml(writer);

            writer.WriteEndElement();
        }
    }
}