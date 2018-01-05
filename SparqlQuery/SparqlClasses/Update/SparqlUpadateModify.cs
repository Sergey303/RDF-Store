using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;
using System.Xml;
using System.Xml.Schema;

namespace SparqlQuery.SparqlClasses.Update
{
    public class SparqlUpdateModify : ISparqlUpdate    
    {
        private readonly RdfQuery11Translator q;
        private ISparqlGraphPattern @where;
        private ObjectVariants with;
        private SparqlQuadsPattern insert;
        private SparqlQuadsPattern delete;

        public void SetWith(ObjectVariants iri)
        {
            this.with = iri;
        }

        public SparqlUpdateModify(RdfQuery11Translator q)
        {
            this.q = q;
        }

        public SparqlUpdateModify(SparqlQuadsPattern sparqlUpdateDelete)
        {
            this.delete = sparqlUpdateDelete;
        }
     

        internal void SetInsert(SparqlQuadsPattern sparqlUpdateInsert)
        {
            this.insert = sparqlUpdateInsert;
        }

        internal void SetDelete(SparqlQuadsPattern sparqlUpdateDelete)
        {
            this.delete = sparqlUpdateDelete;
        }

        internal void SetWhere(ISparqlGraphPattern sparqlGraphPattern)
        {
            this.@where = sparqlGraphPattern;
        }

        public void Run(IStore store)
        {
            var results = this.@where.Run(Enumerable.Repeat(new SparqlResult(this.q), 1));
            SparqlTriple[] defaultGraphTriplesInsert = null;
            SparqlTriple[] defaultGraphTriplesDelete = null;
            SparqlGraphGraph[] graphPatternsInsert = null;
            SparqlGraphGraph[] graphPatternsDelete=null;
            if (this.insert != null)
            {                  
                defaultGraphTriplesInsert = this.insert.Where(pattern => pattern.PatternType == SparqlGraphPatternType.SparqlTriple)
                        .Cast<SparqlTriple>()
                        .ToArray();
                graphPatternsInsert = this.insert
                    .Where(pattern => pattern.PatternType == SparqlGraphPatternType.Graph)
                    .Cast<SparqlGraphGraph>()
                    .ToArray();
            }

            if (this.delete != null)
            {
                defaultGraphTriplesDelete = this.delete.Where(pattern => pattern.PatternType == SparqlGraphPatternType.SparqlTriple)
                        .Cast<SparqlTriple>()
                        .ToArray();

                graphPatternsDelete = this.delete
                    .Where(pattern => pattern.PatternType == SparqlGraphPatternType.Graph)
                    .Cast<SparqlGraphGraph>()
                    .ToArray();
            }

            foreach (var result in results)
            {
               
                if (this.delete != null)
                {
                    if (this.with == null)
                        foreach (SparqlTriple triple in defaultGraphTriplesDelete)
                            triple.Substitution(result,
                                store.Delete);
                    else
                        foreach (SparqlTriple triple in defaultGraphTriplesDelete)
                            triple.Substitution(result, this.with,
                                store.NamedGraphs.Delete);
                    foreach (SparqlGraphGraph sparqlGraphPattern in graphPatternsDelete)
                    {
                        if (sparqlGraphPattern.Name is VariableNode)
                        {
                            var gVariableNode = (VariableNode)sparqlGraphPattern.Name;
                            foreach (var triple in sparqlGraphPattern.GetTriples())
                                triple.Substitution(result, gVariableNode, store.NamedGraphs.Delete);
                        }
                        else
                            foreach (var triple in sparqlGraphPattern.GetTriples())
                                triple.Substitution(result, sparqlGraphPattern.Name, store.NamedGraphs.Delete);
                    }
                }

                if (this.insert != null)
                {
                    if (this.with == null)
                    foreach (SparqlTriple triple in defaultGraphTriplesInsert)
                        triple.Substitution(result,
                            store.Add);
                    else
                        foreach (SparqlTriple triple in defaultGraphTriplesInsert)
                            triple.Substitution(result, this.with,
                                store.NamedGraphs.Add);
                    foreach (SparqlGraphGraph sparqlGraphPattern in graphPatternsInsert)
                    {
                        if (sparqlGraphPattern.Name is VariableNode)
                        {
                            var gVariableNode = (VariableNode) sparqlGraphPattern.Name;
                            foreach (var triple in sparqlGraphPattern.GetTriples())
                                triple.Substitution(result, gVariableNode, store.NamedGraphs.Add);
                        }
                        else
                            foreach (var triple in sparqlGraphPattern.GetTriples())
                                triple.Substitution(result, sparqlGraphPattern.Name, store.NamedGraphs.Add);
                    }
                }
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            @where = (ISparqlGraphPattern) Query.SparqlQuery.CreateByTypeAttribute(reader);
            with = (ObjectVariants) Query.SparqlQuery.CreateByTypeAttribute(reader);
            insert = (SparqlQuadsPattern) Query.SparqlQuery.CreateByTypeAttribute(reader);
            delete =  (SparqlQuadsPattern) Query.SparqlQuery.CreateByTypeAttribute(reader);

        }

        public void WriteXml(XmlWriter writer)
        {
            
            writer.WriteStartElement("modify");
            writer.WriteAttributeString("type", this.GetType().ToString());
            @where.WriteXml(writer);
            with.WriteXml(writer);
            insert.WriteXml(writer);
            delete.WriteXml(writer);
            writer.WriteEndElement();
        
    }
    }
}
