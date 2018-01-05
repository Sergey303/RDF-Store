using System;
using System.Collections.Generic;
using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;

namespace RDFStore
{
    public class RdfNamedGraphs :IStoreNamedGraphs
    {
        protected readonly Dictionary<string,IGraph> named=new Dictionary<string, IGraph>();
        private NodeGenerator ng;

        public RdfNamedGraphs(NodeGenerator ng, Func<string, IGraph> graphCtor, Action<string> graphDrop)
        {
            getGraphUriByName = g => this.ng.GetUri(g.Name);
            this.ng = ng;
            this.graphCtor = graphCtor;
            this.graphDrop = graphDrop;
        }

        public IEnumerable<ObjectVariants> GetPredicate(ObjectVariants subjectNode, ObjectVariants objectNode, ObjectVariants graph)
        {
            IGraph g;
            if (named.TryGetValue(graph.ToString(), out g)) return Enumerable.Empty<ObjectVariants>();
            return g.GetTriplesWithSubjectObject(subjectNode, objectNode);
        }

        public IEnumerable<ObjectVariants> GetSubject(ObjectVariants predicateNode, ObjectVariants objectNode, ObjectVariants graph)
        {
            IGraph g;
            if (named.TryGetValue(graph.ToString(), out g)) return Enumerable.Empty<ObjectVariants>();
            return g.GetSubjects(predicateNode, objectNode);
        }

        public IEnumerable<ObjectVariants> GetObject(ObjectVariants subjectNode, ObjectVariants predicateNode, ObjectVariants graph)
        {
            IGraph g;
            if (named.TryGetValue(graph.ToString(), out g)) return Enumerable.Empty<ObjectVariants>();
            return g.GetTriplesWithSubjectPredicate(subjectNode, predicateNode);
        }

        public IEnumerable<ObjectVariants> GetGraph(ObjectVariants subjectNode, ObjectVariants predicateNode, ObjectVariants objectNode)
        {

            return named.Values.Where(g => g.Contains(subjectNode, predicateNode, objectNode)).Select(getGraphUriByName);
        }

        private readonly Func<IGraph, ObjectVariants> getGraphUriByName;
        private readonly Func<string, IGraph> graphCtor;
        private readonly Action<string> graphDrop;

        public IEnumerable<QuadOVStruct> GetTriplesWithSubjectPredicate(ObjectVariants subjectNode, ObjectVariants predicateNode)
        {
            return named.Values.SelectMany(g => g.GetTriplesWithSubjectPredicate(subjectNode, predicateNode).Select(o => new QuadOVStruct(null, null, o, getGraphUriByName(g))));
        }

        public IEnumerable<QuadOVStruct> GetTriplesWithPredicateObject(ObjectVariants predicateNode, ObjectVariants objectNode)
        {
            return named.Values.SelectMany(g => g.GetSubjects(predicateNode, objectNode).Select(s => new QuadOVStruct(s, null, null, getGraphUriByName(g))));
        }

        public IEnumerable<QuadOVStruct> GetTriplesWithSubjectObject(ObjectVariants subjectNode, ObjectVariants objectNode)
        {
            return named.Values.SelectMany(g => g.GetTriplesWithSubjectObject(subjectNode, objectNode).Select(p => new QuadOVStruct(null, p, null, getGraphUriByName(g))));
        }

        public IEnumerable<QuadOVStruct> GetTriplesWithSubjectFromGraph(ObjectVariants subjectNode, ObjectVariants graph)
        {
            IGraph g;
            if (!named.TryGetValue(graph.ToString(), out g)) return Enumerable.Empty<QuadOVStruct>();
            return g.GetTriplesWithSubject(subjectNode).Select(t => new QuadOVStruct(null, t.Predicate, t.Object, getGraphUriByName(g)));
        }

        public IEnumerable<QuadOVStruct> GetTriplesWithPredicateFromGraph(ObjectVariants predicateNode, ObjectVariants graph)
        {
            IGraph g;                                     
            if (!named.TryGetValue(graph.ToString(), out g)) return Enumerable.Empty<QuadOVStruct>();
            return
                g.GetTriplesWithPredicate(predicateNode)
                    .Select(t => new QuadOVStruct(null, t.Predicate, t.Object, getGraphUriByName(g)));
        }

        public void Add(ObjectVariants graph, IEnumerable<TripleOV> enumerable)
        {
            IGraph g;
            if (!named.TryGetValue(graph.ToString(), out g)) return;
            g.Add(enumerable);
        }

        public IEnumerable<QuadOVStruct> GetTriplesWithObjectFromGraph(ObjectVariants objectNode, ObjectVariants graph)
        {                     
            IGraph g;
            if (!named.TryGetValue(graph.ToString(), out g)) return Enumerable.Empty<QuadOVStruct>();
            return g.GetTriplesWithObject(objectNode)
                    .Select(t => new QuadOVStruct(t.Subject, t.Predicate, null, getGraphUriByName(g)));
        }

        public IEnumerable<QuadOVStruct> GetTriplesWithPredicate(ObjectVariants predicateNode)
        {
            return named.Values.SelectMany(g => g.GetTriplesWithPredicate(predicateNode).Select(t => new QuadOVStruct(t.Subject, null, t.Object, getGraphUriByName(g))));
        }

        public IEnumerable<QuadOVStruct> GetTriplesWithObject(ObjectVariants objectNode)
        {
            return named.Values.SelectMany(g => g.GetTriplesWithObject(objectNode).Select(t => new QuadOVStruct(t.Subject, t.Predicate, null, getGraphUriByName(g))));
        }

        public IEnumerable<QuadOVStruct> GetTriplesWithSubject(ObjectVariants subjectNode)
        {
            return named.Values.SelectMany(g => g.GetTriplesWithSubject(subjectNode).Select(t => new QuadOVStruct(null, t.Predicate, t.Object, getGraphUriByName(g))));
        }

        public IEnumerable<T> GetTriplesFromGraph<T>(ObjectVariants graph, Func<ObjectVariants, ObjectVariants, ObjectVariants, T> returns)
        {
            IGraph g;
            if (!named.TryGetValue(graph.ToString(), out g)) return Enumerable.Empty<T>();
            return g.GetTriples(returns);
        }

        public IGraph CreateGraph(string g)
        {
            var graph = graphCtor(g);
            graph.Name = g;
            named.Add(g, graph);
            return graph;
        }

        public bool Contains(ObjectVariants sValue, ObjectVariants pValue, ObjectVariants oValue, ObjectVariants graph)
        {
            return named.Values.Any(g => g.Contains(sValue, pValue, oValue));
        }

        public void DropGraph(string g)
        {
            named.Remove(g);
            graphDrop(g);
        }

        public void Clear(string uri)
        {
            IGraph g;
            if(named.TryGetValue(uri, out g))
                g.Clear();
        }

        public void Delete(ObjectVariants g, ObjectVariants s, ObjectVariants p, ObjectVariants o)
        {
            IGraph graph;
            if (named.TryGetValue(g.ToString(), out graph))
                graph.Delete(s, p, o);
        }

        //public void DeleteFromAll(IEnumerable<TripleOV> triples)
        //{
        //    throw new NotImplementedException();
        //}

        public void Add(ObjectVariants g, ObjectVariants s, ObjectVariants p, ObjectVariants o)
        {
            IGraph graph;
            if (named.TryGetValue(g.ToString(), out graph))
                graph.Add(s, p, o);
        }

        public void AddGraph(string to, IGraph fromGraph)
        {
          named.Add(to, fromGraph);
        }

      
        public IEnumerable<KeyValuePair<string, long>> GetAllGraphCounts()
        {
            return named.Select(pair => new KeyValuePair<string, long>(pair.Key, pair.Value.GetTriplesCount()));
        }

        public IGraph GetGraph(string graphUriNode)
        {
            throw new NotImplementedException();
        }

      

        public bool Any(string graphUri)
        {
            throw new NotImplementedException();
        }

        public void ClearAllNamedGraphs()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll<T>(Func<ObjectVariants, ObjectVariants, ObjectVariants, ObjectVariants, T> func)
        {
            throw new NotImplementedException();
        }
    }
}