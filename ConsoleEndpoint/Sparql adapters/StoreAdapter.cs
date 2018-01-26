namespace ConsoleEndpoint.Sparql_adapters
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    using ConsoleEndpoint.Interface;

    using RDFCommon;
    using RDFCommon.Interfaces;

    using oldStore= RDFCommon.Interfaces.IStore;
    using newStore= ConsoleEndpoint.Interface.IStore;
    using RDFCommon.OVns;

    class StoreAdapter: oldStore
    {
        private newStore newStore;

        private INametable nametable;
        public string Name { get; set; } = "new store";

        public NodeGenerator NodeGenerator { get; }

        public void Add(ObjectVariants s, ObjectVariants p, ObjectVariants o)
        {
            throw new NotImplementedException();
        }

        public bool Any()
        {
            return newStore.SPO().Any();
        }

        public void Build(long nodesCount, IEnumerable<TripleStrOV> triples)
        {
            newStore.Load(triples.Select(t => new object[]
                                                  {
                                                      this.nametable.GetSetCode(t.Subject),
                                                      this.nametable.GetSetCode(t.Predicate),
                                                      new object[]{ t.Object.Variant== ObjectVariantEnum.Iri ? 1 : 2, t.Object.WritableValue }, 
                                                  }));
        }

        public void Clear()
        {
            this.nametable.Clear();
            // todo
        }

        public bool Contains(ObjectVariants subject, ObjectVariants predicate, ObjectVariants obj)
        {
           return newStore.Contains(
               (int)subject.WritableValue,
               (int)predicate.WritableValue,
                new[] { obj.Variant == ObjectVariantEnum.IriInt ? 1 : 2, obj.WritableValue });
        }

        public void Delete(ObjectVariants subject, ObjectVariants predicate, ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public void AddFromTurtle(long iri_Count, string gString)
        {
            throw new NotImplementedException();
        }

        public void FromTurtle(string fullName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ObjectVariants> GetAllSubjects()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ObjectVariants> GetTriplesWithPredicateObject(ObjectVariants pred, ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetTriples<T>(Func<ObjectVariants, ObjectVariants, ObjectVariants, T> returns)
        {
            throw new NotImplementedException();
        }

        public long GetTriplesCount()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithObject(ObjectVariants o)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithTextObject(ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithPredicate(ObjectVariants p)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithSubject(ObjectVariants s)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ObjectVariants> GetTriplesWithSubjectObject(ObjectVariants subj, ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ObjectVariants> GetTriplesWithSubjectPredicate(ObjectVariants subj, ObjectVariants pred)
        {
            throw new NotImplementedException();
        }

        public void Warmup()
        {
            throw new NotImplementedException();
        }

        public void FromTurtle(Stream requestInputStream)
        {
            throw new NotImplementedException();
        }

        public void ClearAll()
        {
            throw new NotImplementedException();
        }

        public IGraph CreateTempGraph()
        {
            throw new NotImplementedException();
        }

        public void ReloadFrom(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
