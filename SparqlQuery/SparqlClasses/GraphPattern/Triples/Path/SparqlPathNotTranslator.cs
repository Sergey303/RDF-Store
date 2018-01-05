using System.Collections.Generic;
using System.Linq;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.Expressions;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Path
{
    using System.Xml;

    using SparqlQuery.SparqlClasses.Query;

    public class SparqlPathNotTranslator : SparqlPathTranslator
    {
        public readonly List<SparqlPathTranslator> alt=new List<SparqlPathTranslator>();

        /// <summary>
        /// 4 serialization only
        /// </summary>
        public SparqlPathNotTranslator() : base()
        {
            
        }

        public SparqlPathNotTranslator(SparqlPathTranslator path)
            : base(path.predicate)
        {
            // TODO: Complete member initialization
            this.alt.Add(path); 
        }
                                               
        public override IEnumerable<ISparqlGraphPattern> CreateTriple(ObjectVariants subject, ObjectVariants @object, RdfQuery11Translator q)
        {
                var subjectNode = this.IsInverse ? @object : subject;
           var objectNode = this.IsInverse ? subject : @object;
            var directed = this.alt.Where(translator => !translator.IsInverse).ToList();
            var anyDirected = directed.Any();
            if (anyDirected)
            {
                var variableNode = q.CreateBlankNode();
                yield return new SparqlTriple((ObjectVariants) subjectNode, variableNode, (ObjectVariants) objectNode, q);
                foreach (var pathTranslator in directed)
                    yield return
                        new SparqlFilter(new SparqlNotEqualsExpression(new SparqlVarExpression(variableNode),
                            new SparqlIriExpression(pathTranslator.predicate), q.Store.NodeGenerator));
            }

            // TODO drop subject object variables
            var inversed = this.alt.Where(translator => translator.IsInverse).ToList();
            var anyInversed = inversed.Any();
            if (anyInversed)
            {
                var variableNode =q.CreateBlankNode();
                yield return new SparqlTriple((ObjectVariants) objectNode, variableNode, (ObjectVariants) subjectNode, q);
                foreach (var pathTranslator in inversed)
                    yield return
                        new SparqlFilter(new SparqlNotEqualsExpression(new SparqlVarExpression(variableNode),
                            new SparqlIriExpression(pathTranslator.predicate), q.Store.NodeGenerator));
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            IsInverse = bool.Parse(reader.GetAttribute("inverse"));
            while (reader.IsStartElement())
            {
                this.alt.Add((SparqlPathTranslator)SparqlQuery.CreateByTypeAttribute(reader));
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("NotPath");
            writer.WriteAttributeString("type", this.GetType().ToString());

            writer.WriteAttributeString("inverse", IsInverse.ToString());
            foreach (var pathTranslator in this.alt)
            {
                pathTranslator.WriteXml(writer);
            }

            writer.WriteEndElement();
        }
    }
}
