using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleEndpoint.old_sparql_new_store
{
    using System.IO;
    using System.Linq;

    using RDFCommon;
    using RDFCommon.Interfaces;
    using RDFCommon.OVns;
    using RDFCommon.OVns.general;

    class Store4oldSparqlAdapter :IStore
    {
        private readonly Interfaces.IStore strore;

        public Store4oldSparqlAdapter(ConsoleEndpoint.Interfaces.IStore strore)
        {
            this.strore = strore;
        }
        public string Name { get; set; }

        public NodeGenerator NodeGenerator { get; }

        public void Add(ObjectVariants s, ObjectVariants p, ObjectVariants o)
        {
            throw new NotImplementedException();
        }

        public bool Any()
        {
            return this.strore.SPO().Any();
        }

        public void Build(long nodesCount, IEnumerable<TripleStrOV> triples)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ObjectVariants subject, ObjectVariants predicate, ObjectVariants obj)
        {
            return this.strore.Contains(
                (int)subject.Content,
                (int)predicate.Content,
                new[] { (int)obj.Variant, obj.Content });
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
            var objects = obj.Variant == ObjectVariantEnum.IriInt ? new[] { OVT.iri, (object)obj.Content } : new[] { OVT.@string, (object)obj.Content };
            return this.strore.Spo((int)pred.Content, objects)
                .Select(s => new OV_iriint((int)s[0], this.strore.Nametable.GetString));
        }

        public IEnumerable<T> GetTriples<T>(Func<ObjectVariants, ObjectVariants, ObjectVariants, T> returns)
        {
            return from triple in this.strore.SPO()
                   select returns(
                       this.obj2IriInt(triple[0]),
                       this.obj2IriInt(triple[1]),
                       this.GetObjectOV((object[])triple[2]));
        }

        private ObjectVariants GetObjectOV(object[] o)
        {
            return o[0].Equals(0)
                       ? (ObjectVariants)new OV_iriint((int)o[1], this.strore.Nametable.GetString)
                       : new OV_string((string)o[1]);
        }

        public long GetTriplesCount()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithObject(ObjectVariants o)
        {
            var objects = o.Variant == ObjectVariantEnum.IriInt ? new[] { OVT.iri, (object)o.Content } : new[] { OVT.@string, (object)o.Content };
            return this.strore.SPo(objects)
                .Select(t=> new TripleOVStruct(this.obj2IriInt(t[0]), this.obj2IriInt(t[1]), this.GetObjectOV((object[])t[2])));
        }

        private OV_iriint obj2IriInt(object t)
        {
            return new OV_iriint((int)t, this.strore.Nametable.GetString);
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithTextObject(ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithPredicate(ObjectVariants p)
        {
     
            return this.strore.SpO((int)p.Content)
                .Select(t => new TripleOVStruct(this.obj2IriInt(t[0]), this.obj2IriInt(t[1]), this.GetObjectOV((object[])t[2])));

        }

        public IEnumerable<TripleOVStruct> GetTriplesWithSubject(ObjectVariants s)
        {
            return this.strore.sPO((int)s.Content)
                .Select(t => new TripleOVStruct(this.obj2IriInt(t[0]), this.obj2IriInt(t[1]), this.GetObjectOV((object[])t[2])));

        }

        public IEnumerable<ObjectVariants> GetTriplesWithSubjectObject(ObjectVariants subj, ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ObjectVariants> GetTriplesWithSubjectPredicate(ObjectVariants subj, ObjectVariants pred)
        {
            return this.strore.spO((int)subj.Content, (int)pred.Content)
                .Select(t=>(object[])t[2])
                .Select(this.GetObjectOV);
        }

        public void Warmup()
        {
            throw new NotImplementedException();
        }

        public void FromTurtle(Stream requestInputStream)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<TripleOV> enumerable)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<TripleStrOV> selectMany)
        {
            throw new NotImplementedException();
        }

        public IStoreNamedGraphs NamedGraphs { get; }

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
