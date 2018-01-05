using System.Collections.Generic;
using System.Linq;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;
using SparqlQuery.SparqlClasses.SolutionModifier;

namespace SparqlQuery.SparqlClasses.Query
{
    using System.Xml;

    public class SparqlDescribeQuery : SparqlQuery
    {
        private readonly List<ObjectVariants> nodeList = new List<ObjectVariants>();
        private bool isAll;


        public SparqlDescribeQuery(RdfQuery11Translator q) : base(q)
        {
            
        }

        internal void Add(ObjectVariants sparqlNode)
        {
            this.nodeList.Add(sparqlNode);
        }

        internal void IsAll()
        {
            this.isAll = true;
        }

        internal void Create(ISparqlGraphPattern sparqlWhere)
        {
            this.sparqlWhere = sparqlWhere;
        }

        internal void Create(SparqlSolutionModifier sparqlSolutionModifier)
        {
            this.SparqlSolutionModifier = sparqlSolutionModifier;
        }

        public override SparqlResultSet Run()
         {
            base.Run();
            var rdfInMemoryGraph = this.q.Store.CreateTempGraph();
            if (this.isAll)
                foreach (ObjectVariants node in this.ResultSet.Results.SelectMany(result => this.q.Variables.Values.Select(v=>result[v])))
                {
                    // .Where(node => node is ObjectVariants).Cast<ObjectVariants>()))
                    ObjectVariants node1 = node;
                    var ovIri = node as OV_iri;
                    if (ovIri != null)
                        if (!this.q.Store.NodeGenerator.TryGetUri(ovIri, out node1)) continue;
                    foreach (var t in this.q.Store.GetTriplesWithSubject(node1))
                    {
                        rdfInMemoryGraph.Add(node1, t.Predicate, t.Object);
                    }

                    foreach (var t in this.q.Store.GetTriplesWithObject(node1))
                    {
                        rdfInMemoryGraph.Add(t.Subject, t.Predicate, node1);
                    }
                }
            else
            {
                foreach (ObjectVariants node in this.nodeList
                    .Where(node => node is VariableNode)
                    .Cast<VariableNode>()
                    .SelectMany(v=> this.ResultSet.Results.Select(result => result[v])))
                {
                    // .Where(node => node is ObjectVariants).Cast<ObjectVariants>()
                    ObjectVariants node1 = node;
                    var ovIri = node as OV_iri;
                    if (ovIri != null)
                        if (!this.q.Store.NodeGenerator.TryGetUri(ovIri, out node1)) continue;
                    foreach (var t in this.q.Store.GetTriplesWithSubject(node1))
                    {
                        rdfInMemoryGraph.Add(node1, t.Predicate, t.Object);
                    }

                    foreach (var t in this.q.Store.GetTriplesWithObject(node1))
                    {
                        rdfInMemoryGraph.Add(t.Subject, t.Predicate, node1);
                    }
                }

                foreach (ObjectVariants node in this.nodeList.Where(node => !(node is VariableNode)))
                    {
                    ObjectVariants node1 = node;
                        var ovIri = node as OV_iri;
                        if (ovIri != null)
                        if(!this.q.Store.NodeGenerator.TryGetUri(ovIri, out node1)) continue;
                        foreach (var t in this.q.Store.GetTriplesWithSubject(node1))
                        {
                            rdfInMemoryGraph.Add(node1, t.Predicate, t.Object);
                        }

                        foreach (var t in this.q.Store.GetTriplesWithObject(node1))
                        {
                            rdfInMemoryGraph.Add(t.Subject, t.Predicate, node1);
                        }
                    }
            }

             this.ResultSet.ResultType = ResultType.Describe;
             this.ResultSet.GraphResult = rdfInMemoryGraph;
            return this.ResultSet;
        }
      
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("describe");

            base.WriteXml(writer);

            writer.WriteEndElement();
        }
        // public override SparqlQueryTypeEnum QueryType
        // {
        // get { return SparqlQueryTypeEnum.Describe; }
        // }
    }
}
