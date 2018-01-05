using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFTurtleParser;

namespace RDFStore
{
    public class RamListOfTriplesGraph : NodeGenerator, IGraph 
    {
        public RamListOfTriplesGraph(string name)
        {
          //  Name = Guid.NewGuid().ToString();
            Name = name;
            
        }
        private readonly List<TripleOV> triples=new List<TripleOV>();

        public RamListOfTriplesGraph()
        {
            SpecialTypes=new SpecialTypesClass(this);
        }

        public IEnumerable<ObjectVariants> GetTriplesWithSubjectPredicate(ObjectVariants subjectNode, ObjectVariants predicateNode)
        {
            return triples.Where(triple => triple.Subject .Equals( subjectNode) && triple.Predicate .Equals( predicateNode)).Select(triple => triple.Object);
        }

        public IEnumerable<ObjectVariants> GetTriplesWithSubjectObject(ObjectVariants subjectNode, ObjectVariants objectNode)
        {
            return triples.Where(triple => triple.Subject .Equals( subjectNode) && triple.Object .Equals( objectNode)).Select(triple => triple.Predicate);

        }

        public IEnumerable<TripleOVStruct> GetTriplesWithSubject(ObjectVariants subjectNode)
        {
            return triples.Where(triple => triple.Subject.Equals(subjectNode)).Select(triple =>  new TripleOVStruct(null, triple.Predicate, triple.Object));
        }

        public IEnumerable<ObjectVariants> GetSubjects(ObjectVariants predicateNode, ObjectVariants objectNode)
        {
            return triples.Where(triple => triple.Predicate .Equals( predicateNode) && triple.Object .Equals( objectNode)).Select(triple => triple.Subject);
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithTextObject(ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithPredicate(ObjectVariants predicateNode)
        {
            return triples.Where(triple => triple.Predicate .Equals( predicateNode)).Select(triple => new TripleOVStruct(triple.Subject, null, triple.Object));
        }



        public IEnumerable<TripleOVStruct> GetTriplesWithObject(ObjectVariants o)
        {
            return triples.Where(triple => triple.Object.Equals(o)).Select(triple => new TripleOVStruct(triple.Subject, triple.Predicate, null)); 
        }

        public IEnumerable<T> GetTriples<T>(Func<ObjectVariants, ObjectVariants, ObjectVariants, T> returns)
        {
            return triples.Select(triple => returns(triple.Subject, triple.Predicate, triple.Object));
        }

        public bool Contains(ObjectVariants subject, ObjectVariants predicate, ObjectVariants @object)
        {
           return triples.Any(triple => triple.Subject.Equals(subject) && triple.Predicate.Equals(predicate) && triple.Object.Equals(@object));
        }

        public void Delete(ObjectVariants s, ObjectVariants p, ObjectVariants o)
        {
            foreach (var t in triples.Where(triple => triple.Subject.Equals(s) && triple.Predicate.Equals(p) && triple.Object.Equals(o)).ToArray())
            {
                triples.Remove(t);    
            }
            
        }

        public void AddFromTurtle(long iri_Count, string gString)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<ObjectVariants> GetAllSubjects()
        {
            return triples.Select(t => t.Subject).Distinct();
        }

        public long GetTriplesCount()
        {
            return triples.Count;
        }

        public bool Any()
        {
            throw new NotImplementedException();
        }

        public void FromTurtle(long count, string fileName)
        {
            var generator = new TripleGeneratorBufferedParallel(fileName, Name);
            generator.Start(list => triples.AddRange(
                list.Select(
                    t =>    
                        new TripleOV(
                            NodeGenerator.AddIri(t.Subject),
                            NodeGenerator.AddIri(t.Predicate), 
                            (ObjectVariants) t.Object))));
        }

        public void Build(IEnumerable<TripleStrOV> triples)
        {
            throw new NotImplementedException();
        }

        public void Build(long nodesCount, IEnumerable<TripleStrOV> triples)
        {
            throw new NotImplementedException();
        }

        public void FromTurtle(Stream inputStream)
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

        public void Warmup()
        {
            
        }


        public string Name { get; set; }
        public NodeGenerator NodeGenerator { get { return this; }}

        public void Clear()
        {
           triples.Clear();
        }

    
        public void Add(ObjectVariants s, ObjectVariants p, ObjectVariants o)
        {
           triples.Add(new TripleOV(s,p,o));
        }

    
        //public void LoadFrom(IUriNode @from)
        //{
        //    throw new NotImplementedException();
        //}

      

        public void FromTurtle(string fullName)
        {
           FromTurtle(1000*1000, fullName);
        }
    }
}
