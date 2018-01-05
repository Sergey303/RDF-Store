using System;
using System.Collections.Generic;
using RDFCommon.OVns;

namespace RDFCommon.Interfaces
{
    public interface IStoreNamedGraphs
    {
        IEnumerable<ObjectVariants> GetPredicate(ObjectVariants subjectNode, ObjectVariants objectNode, ObjectVariants graph);
        IEnumerable<ObjectVariants> GetSubject(ObjectVariants predicateNode, ObjectVariants objectNode, ObjectVariants graph);
        IEnumerable<ObjectVariants> GetObject(ObjectVariants subjectNode, ObjectVariants predicateNode, ObjectVariants graph);
        IEnumerable<ObjectVariants> GetGraph(ObjectVariants subjectNode, ObjectVariants predicateNode, ObjectVariants objectNode);

        IEnumerable<QuadOVStruct> GetTriplesWithSubjectPredicate(ObjectVariants subjectNode, ObjectVariants predicateNode);
        IEnumerable<QuadOVStruct> GetTriplesWithPredicateObject(ObjectVariants predicateNode, ObjectVariants objectNode);
        IEnumerable<QuadOVStruct> GetTriplesWithSubjectObject(ObjectVariants subjectNode, ObjectVariants objectNode);

        IEnumerable<QuadOVStruct> GetTriplesWithSubjectFromGraph(ObjectVariants subjectNode, ObjectVariants graph);
        IEnumerable<QuadOVStruct> GetTriplesWithPredicateFromGraph(ObjectVariants predicateNode, ObjectVariants graph);
        void Add(ObjectVariants name, IEnumerable<TripleOV> enumerable);
        IEnumerable<QuadOVStruct> GetTriplesWithObjectFromGraph(ObjectVariants objectNode, ObjectVariants graph);

        IEnumerable<QuadOVStruct> GetTriplesWithPredicate(ObjectVariants predicateNode);
        IEnumerable<QuadOVStruct> GetTriplesWithObject(ObjectVariants objectNode);
        IEnumerable<QuadOVStruct> GetTriplesWithSubject(ObjectVariants subjectNode);
        IEnumerable<T> GetTriplesFromGraph<T>(ObjectVariants graph, Func<ObjectVariants, ObjectVariants, ObjectVariants, T> returns);

        IGraph CreateGraph(string sparqlUriNode);
        bool Contains(ObjectVariants sValue, ObjectVariants pValue, ObjectVariants oValue, ObjectVariants graph);
        void DropGraph(string sparqlGrpahRefTypeEnum);
        void Clear(string uri);

        void Delete(ObjectVariants g, ObjectVariants s, ObjectVariants p, ObjectVariants o);
        //void DeleteFromAll(IEnumerable<TripleOV> triples);
        void Add(ObjectVariants g, ObjectVariants s, ObjectVariants p, ObjectVariants o);

      //  IGraph TryGetGraph(IUriNode graphUriNode);

       //  Dictionary<IUriNode,IGraph> Named { get;  }
        void AddGraph(string to, IGraph fromGraph);
        //void ReplaceGraph(ObjectVariants to, IGraph graph);
        IEnumerable<KeyValuePair<string, long>> GetAllGraphCounts();

       // bool ContainsGraph(IUriNode to);

        IGraph GetGraph(string graphUriNode);
     //   IEnumerable<ObjectVariants> GetAllSubjects(ObjectVariants graphUri);
        bool Any(string graphUri);

        void ClearAllNamedGraphs();
        IEnumerable<T> GetAll<T>(Func<ObjectVariants, ObjectVariants, ObjectVariants, ObjectVariants, T> func);
    }
}