using System.Collections.Generic;
using System.Linq;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Path
{
    using System;
    using System.Xml;

    public class SparqlPathZeroOrMany : SparqlPathTranslator
    {
        private readonly SparqlPathTranslator path;

        public SparqlPathZeroOrMany(SparqlPathTranslator path)
            : base(path.predicate)
        {
            this.path = path;
        }


                                          
        public override IEnumerable<ISparqlGraphPattern> CreateTriple(ObjectVariants subject, ObjectVariants @object, RdfQuery11Translator q)
        {
            var subjectNode = this.IsInverse ? @object : subject;
            var objectNode = this.IsInverse ? subject : @object;

            var sparqlPathManyTriple = new SparqlPathManyTriple(subjectNode, this.path, objectNode, q);
           
           yield return new SparqlMayBeOneTriple(Enumerable.Repeat(sparqlPathManyTriple, 1), subjectNode, objectNode, q);
        }
        public override void WriteXml(XmlWriter writer)
        {
            throw new Exception();
        }
    }
}