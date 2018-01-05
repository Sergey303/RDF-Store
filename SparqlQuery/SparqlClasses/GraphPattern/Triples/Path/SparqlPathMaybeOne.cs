using System.Collections.Generic;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Path
{
    using System.Xml;

    public class SparqlPathMaybeOne : SparqlPathTranslator
    {
        private SparqlPathTranslator path;

        public SparqlPathMaybeOne(SparqlPathTranslator path) : base(path.predicate)
        {
            this.path = path;
        }
                                                    
        public override IEnumerable<ISparqlGraphPattern> CreateTriple(ObjectVariants subject, ObjectVariants @object, RdfQuery11Translator q)
        {
            var subjectNode = this.IsInverse ? @object : subject;
            var objectNode = this.IsInverse ? subject : @object;

            // if (subjectNode is ObjectVariants && objectNode is ObjectVariants)
            yield return
                new SparqlMayBeOneTriple(
                    this.path.CreateTriple((ObjectVariants)subjectNode, objectNode, q),
                    (ObjectVariants)subjectNode,
                    (ObjectVariants)objectNode,
                    q);

            // else                                                                                             
            // foreach (var t in path.CreateTriple((ObjectVariants) subjectNode, objectNode, q))
            // yield return t;
        }

        public override void ReadXml(XmlReader reader)
        {
            this.path = (SparqlPathTranslator)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader);

        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("mayBeOnePath");
        
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.path.WriteXml(writer);

            writer.WriteEndElement();
        }
    }
}