using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples.Path
{
    public class SparqlPathManyTriple : ISparqlGraphPattern
    {
        private SparqlPathTranslator predicatePath;           
        private Dictionary<ObjectVariants, HashSet<ObjectVariants>> bothVariablesCacheBySubject;

        private Dictionary<ObjectVariants, HashSet<ObjectVariants>> bothVariablesCacheByObject;

        private KeyValuePair<ObjectVariants, ObjectVariants>[] bothVariablesChache;
        private VariableNode sVariableNode;
        private VariableNode oVariableNode;
        private RdfQuery11Translator q;
        private bool useCache=false;

        public SparqlPathManyTriple(ObjectVariants subject, SparqlPathTranslator pred, ObjectVariants @object, RdfQuery11Translator q)
        {
            this.predicatePath = pred;
            this.Subject = subject;
            this.Object = @object;
            this.q = q;
            this.sVariableNode = this.Subject as VariableNode;
            this.oVariableNode = this.Object as VariableNode;
        }
        

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            var bindings = variableBindings;

            Queue<ObjectVariants> newSubjects = new Queue<ObjectVariants>();
            ObjectVariants[] fromVariable = null;
            ObjectVariants o=null;
            ObjectVariants s=null;
            switch (NullablePairExt.Get(this.sVariableNode, this.oVariableNode))
            {
                case NP.bothNull:
                    
                    return this.TestSOConnection(this.Subject, this.Object) ? bindings : Enumerable.Empty<SparqlResult>();
                case NP.leftNull:
                    newSubjects.Enqueue(this.Subject);
                    return bindings.SelectMany(binding =>
                    {
                          o = binding[this.oVariableNode];
                        if (o != null)
                            return this.TestSOConnection(this.Subject, o)
                                ? Enumerable.Repeat(binding, 1)
                                : Enumerable.Empty<SparqlResult>();
                        else
                        {
                            if (fromVariable == null)
                                fromVariable = this.GetAllSConnections(this.Subject).ToArray();
                            return fromVariable.Select(node => binding.Add(node, this.oVariableNode));
                        }
                    });
                case NP.rigthNull:
                    return bindings.SelectMany(binding =>
                    {
                        s = binding[this.sVariableNode];
                        if (s != null)
                            return this.TestSOConnection(s, this.Object)
                                ? Enumerable.Repeat(binding, 1)
                                : Enumerable.Empty<SparqlResult>();
                        else
                        {
                            if (fromVariable == null)
                                fromVariable = this.GetAllOConnections(this.Object).ToArray();
                            return fromVariable.Select(node => binding.Add(node, this.sVariableNode));
                        }
                    });
                case NP.bothNotNull:
                  

                    return bindings.SelectMany(binding =>
                    {
                        s = binding[this.sVariableNode];
                        o = binding[this.oVariableNode];
                        switch (NullablePairExt.Get(s, o))
                        {
                            case NP.bothNotNull:
                                if ((this.useCache && this.TestSOConnectionFromCache(s, o)) || this.TestSOConnection(s, o))
                                        return Enumerable.Repeat(binding, 1);
                                    else return Enumerable.Empty<SparqlResult>();
                            case NP.rigthNull:
                                return this.GetAllSConnections(s).Select(node => binding.Add(node, this.oVariableNode));
                                break;
                            case NP.leftNull:
                                return this.GetAllOConnections(o).Select(node => binding.Add(node, this.sVariableNode));
                                break;
                            case NP.bothNull:
                                this.useCache = true;
                                this.bothVariablesChache = this.predicatePath.CreateTriple(this.sVariableNode, this.oVariableNode, this.q)
                      .Aggregate(Enumerable.Repeat(new SparqlResult(this.q), 1),
                          (enumerable, triple) => triple.Run(enumerable))
                      .Select(
                          r =>
                              new KeyValuePair<ObjectVariants, ObjectVariants>(r[this.sVariableNode], r[this.oVariableNode]))
                      .ToArray();
                                return this.bothVariablesCacheBySubject.Keys.SelectMany(this.GetAllSConnections,
                                    (sbj, node) => binding.Add(sbj, this.sVariableNode, node, this.oVariableNode));
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
                 
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateCacheBySubject()
        {
            if (this.bothVariablesCacheBySubject == null)
            {
                this.bothVariablesCacheBySubject =
                    new Dictionary<ObjectVariants, HashSet<ObjectVariants>>();
                foreach (var pair in this.bothVariablesChache)
                {
                    HashSet<ObjectVariants> nodes;
                    if (!this.bothVariablesCacheBySubject.TryGetValue(pair.Key, out nodes))
                        this.bothVariablesCacheBySubject.Add(pair.Key,
                            new HashSet<ObjectVariants>() {pair.Value});
                    else nodes.Add(pair.Value);
                }
            }
        }

        public ObjectVariants Subject { get; private set; }
        public ObjectVariants Object { get; private set; }

        private IEnumerable<ObjectVariants> GetAllSConnections(ObjectVariants subj)
        {
           if(this.useCache) this.CreateCacheBySubject();
            HashSet<ObjectVariants> history = new HashSet<ObjectVariants>() { subj };
            Queue<ObjectVariants> subjects = new Queue<ObjectVariants>();
            subjects.Enqueue(subj);
            while (subjects.Count > 0)
            {
                IEnumerable<ObjectVariants> objects;
                if (this.useCache)
                {
                    HashSet<ObjectVariants> objectsSet;
                    objects = this.bothVariablesCacheBySubject.TryGetValue(subjects.Dequeue(), out objectsSet)
                        ? objectsSet
                        : Enumerable.Empty<ObjectVariants>();}
                else
                    objects = this.RunTriple(subjects.Dequeue(), this.oVariableNode)
                        .Select(sparqlResult => sparqlResult[this.oVariableNode]);
                foreach (var objt in objects
                    .Where(objt =>
                    {
                        var isNewS = !history.Contains(objt);
                        if (isNewS)
                        {
                            history.Add(objt);
                            subjects.Enqueue(objt);
                        }

                        return isNewS;
                    }))
                    yield return objt;}
        }

        private IEnumerable<ObjectVariants> GetAllOConnections(ObjectVariants objt)
        {
            if (this.useCache) this.CreateCacheByObject();
            HashSet<ObjectVariants> history = new HashSet<ObjectVariants>() { objt };
            Queue<ObjectVariants> objects = new Queue<ObjectVariants>();
            objects.Enqueue(objt);

            while (objects.Count > 0)  
            {
                IEnumerable<ObjectVariants> subjects;
                if (this.useCache)
                {
                    HashSet<ObjectVariants> subjectsHashSet;
                    subjects = this.bothVariablesCacheByObject.TryGetValue(objects.Dequeue(), out subjectsHashSet) ? subjectsHashSet : Enumerable.Empty<ObjectVariants>();
                }
                else
                  subjects = this.RunTriple(this.sVariableNode, objects.Dequeue())
                    .Select(sparqlResult => sparqlResult[this.sVariableNode]);
                foreach (var subjt in subjects
                    .Where(subjt =>
                    {
                        var isNewS = !history.Contains(subjt);
                        if (isNewS)
                        {
                            history.Add(subjt);
                            objects.Enqueue(subjt);
                        }

                        return isNewS;
                    }))
                    yield return subjt;
            }
        }

        private void CreateCacheByObject()
        {
            if (this.bothVariablesCacheByObject == null)
            {
                this.bothVariablesCacheByObject = new Dictionary<ObjectVariants, HashSet<ObjectVariants>>();
                foreach (var pair in this.bothVariablesChache)
                {
                    HashSet<ObjectVariants> nodes;
                    if (!this.bothVariablesCacheByObject.TryGetValue(pair.Value, out nodes)) this.bothVariablesCacheByObject.Add(pair.Value, new HashSet<ObjectVariants>() {pair.Key});
                    else nodes.Add(pair.Key);
                }
            }
        }


        private bool TestSOConnection(ObjectVariants sbj, ObjectVariants objct)
        {
            HashSet<ObjectVariants> history = new HashSet<ObjectVariants>() { this.Subject };
            Queue<ObjectVariants> newSubjects = new Queue<ObjectVariants>();
            newSubjects.Enqueue(sbj);
            var subject = newSubjects.Peek();
            if (this.RunTriple(subject, objct).Any())  
                return true;
            var newVariable = (SparqlBlankNode)this.q.CreateBlankNode();
            while (newSubjects.Count > 0)
                if (this.RunTriple(newSubjects.Dequeue(), newVariable)
                    .Select(sparqlResult => sparqlResult[newVariable])
                    .Where(o => !history.Contains(o))
                    .Any(o =>
                    {
                        history.Add(o);
                        newSubjects.Enqueue(o);
                        return this.RunTriple(o, objct).Any();
                    }))    
                    return true;
            return false;
        }

        private bool TestSOConnectionFromCache(ObjectVariants sbj, ObjectVariants objct)
        {
            this.CreateCacheBySubject();
            HashSet<ObjectVariants> history = new HashSet<ObjectVariants>() { this.Subject };
            HashSet<ObjectVariants> objects;
            if (this.bothVariablesCacheBySubject.TryGetValue(sbj, out objects) && objects.Contains(objct))
                return true;
            
            Queue<ObjectVariants> newSubjects = new Queue<ObjectVariants>();
            newSubjects.Enqueue(sbj);

            while (newSubjects.Count > 0)
                if (this.bothVariablesCacheBySubject.TryGetValue(newSubjects.Dequeue(), out objects)
                    && objects
                        .Where(o => !history.Contains(o))
                        .Any(o =>
                        {
                            history.Add(o);
                            newSubjects.Enqueue(o);
                            return this.bothVariablesCacheBySubject.TryGetValue(o, out objects) && objects.Contains(objct);
                        }))
                    return true;
            return false;
        }

        private IEnumerable<SparqlResult> RunTriple(ObjectVariants subject, ObjectVariants objct)
        {                                     
                return this.predicatePath.CreateTriple(subject, objct, this.q).Aggregate(Enumerable.Repeat(new SparqlResult(this.q), 1),
                    (enumerable, triple) => triple.Run(enumerable));
           
        }

        public new SparqlGraphPatternType PatternType
        {
            get { return SparqlGraphPatternType.PathTranslator; }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this.Subject = (ObjectVariants)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader);
            this.predicatePath = (SparqlPathTranslator)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader);
            this.Object = (ObjectVariants)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader);

        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("mayBeOne");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.Subject.WriteXml(writer);
            this.predicatePath.WriteXml(writer);
            this.Object.WriteXml(writer);
          
            writer.WriteEndElement();
        }
    }
}