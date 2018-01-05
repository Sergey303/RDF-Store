using System;
using System.Collections.Generic;
using System.Linq;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses
{
    public class SparqlRdfCollection    
    {
        public List<ObjectVariants> nodes = new List<ObjectVariants>();

        public ObjectVariants GetNode(Action<ObjectVariants, ObjectVariants, ObjectVariants> addTriple, NodeGenerator q)
        {


            ObjectVariants sparqlBlankNodeFirst = q.CreateBlankNode();
                ObjectVariants sparqlBlankNodeNext = q.CreateBlankNode();
            foreach (var node in this.nodes.Take(this.nodes.Count - 1))
            {
                addTriple(sparqlBlankNodeNext, q.SpecialTypes.first, node);
                addTriple(sparqlBlankNodeNext, q.SpecialTypes.rest, sparqlBlankNodeNext = q.CreateBlankNode());
            }

            addTriple(sparqlBlankNodeNext, q.SpecialTypes.first, this.nodes[this.nodes.Count - 1]);
            addTriple(sparqlBlankNodeNext, q.SpecialTypes.rest, q.SpecialTypes.nil);
            return sparqlBlankNodeFirst;
        }
    }
}