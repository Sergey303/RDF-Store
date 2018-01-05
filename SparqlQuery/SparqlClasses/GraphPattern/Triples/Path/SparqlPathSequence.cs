using System.Collections.Generic;
using System.Linq;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Path
{
    using System.Xml;

    public class SparqlPathSequence : SparqlPathTranslator
    {
        public readonly List<SparqlPathTranslator> seq = new List<SparqlPathTranslator>();

        public SparqlPathSequence(SparqlPathTranslator sparqlPathTranslator, SparqlPathTranslator sparqlPathTranslator1) :base(null)
        {
            this.seq = new List<SparqlPathTranslator>() {sparqlPathTranslator, sparqlPathTranslator1};
        }

        internal override SparqlPathTranslator AddSeq(SparqlPathTranslator sparqlPathTranslator)
        {
            this.seq.Add(sparqlPathTranslator);
            return this;
        }

        public override IEnumerable<ISparqlGraphPattern> CreateTriple(ObjectVariants  subject, ObjectVariants @object, RdfQuery11Translator q)
        {
            VariableNode t=q.CreateBlankNode();
            var subjectNode = this.IsInverse ? @object : subject;
            var objectNode = this.IsInverse ? subject : @object;
            return this.seq.First().CreateTriple((ObjectVariants) subjectNode, t, q)
                .Concat(this.seq.Skip(1).Take(this.seq.Count - 2).SelectMany(path => path.CreateTriple(t, t = q.CreateBlankNode(), q)))
                .Concat(this.seq.Last().CreateTriple(t, objectNode, q));
        }

        public override void ReadXml(XmlReader reader)
        {
            IsInverse = bool.Parse(reader.GetAttribute("inverse"));

            while (reader.IsStartElement())
            {
                this.seq.Add((SparqlPathTranslator)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader));
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("SequencePath");
            writer.WriteAttributeString("type", this.GetType().ToString());
            writer.WriteAttributeString("inverse", IsInverse.ToString());
            foreach (var pathTranslator in this.seq)
            {
                pathTranslator.WriteXml(writer);
            }

            writer.WriteEndElement();
        }
    }
}