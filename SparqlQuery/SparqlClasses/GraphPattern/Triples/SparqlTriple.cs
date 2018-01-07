using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using SparqlQuery.SparqlClasses.Expressions;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples
{
    [Serializable]
    public class SparqlTriple : ISparqlGraphPattern
    {
        public ObjectVariants Subject { get; private set; }
        public ObjectVariants Predicate { get; private set; }
        public ObjectVariants Object { get; private set; }
        public DataSet graphs;
        public bool isDefaultGraph;

        private bool isGKnown;

        private VariableNode sVariableNode;
        private VariableNode pVariableNode;
        private VariableNode oVariableNode;
        private VariableDataSet variableDataSet;

        
        private readonly List<SparqlFilter> listOfFilters=new List<SparqlFilter>();
        private bool isFirstCall=true;
        private bool isAny = true;
        private readonly IStore store;

        /// <summary>
        /// 4 serialization only
        /// </summary>
        public SparqlTriple()
        {
            
        }

        public SparqlTriple(ObjectVariants subj, ObjectVariants pred, ObjectVariants obj, RdfQuery11Translator q)
        {
            this.Subject = subj;
            this.Predicate = pred;
            this.Object = obj;

            // if(!(subj is ObjectVariants)) throw new ArgumentException();
            this.graphs = q.ActiveGraphs;

            // this.Graph = graph;
            this.sVariableNode = subj as VariableNode;
            this.pVariableNode = pred as VariableNode;
            this.oVariableNode = obj as VariableNode;
            this.variableDataSet = q.ActiveGraphs as VariableDataSet;
            this.isDefaultGraph = this.variableDataSet == null && this.graphs.Count == 0;

            this.store = q.Store;
        }

        public virtual IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            // var backup = result.BackupMask();
            this.PrepareConstants();
            if (!this.isAny) return Enumerable.Empty<SparqlResult>();
            return variableBindings.SelectMany(this.CreateBindings);

            // result.Restore(backup);
        }

        public SparqlGraphPatternType PatternType { get { return SparqlGraphPatternType.SparqlTriple; } }

        private IEnumerable<SparqlResult> CreateBindings(SparqlResult variableBinding)
        {
            this.Subject = this.sVariableNode != null ? variableBinding[this.sVariableNode] : this.Subject;
            this.Predicate = this.pVariableNode != null ? variableBinding[this.pVariableNode] : this.Predicate;
            this.Object = this.oVariableNode != null ? variableBinding[this.oVariableNode] : this.Object;


            if (!this.PrepareKnownVarValues()) return Enumerable.Empty<SparqlResult>();
            if (!this.isDefaultGraph && this.variableDataSet != null)
            {
                var graphFromVar = variableBinding[this.variableDataSet.Variable];
                this.graphs = graphFromVar != null ? new DataSet() { graphFromVar } : null;
                this.isGKnown = this.graphs != null;
            }
            else {
                this.isGKnown = true;}

            int @case = ((this.Subject != null ? 0 : 1) << 2) | ((this.Predicate != null ? 0 : 1) << 1) | (this.Object != null ? 0 : 1);
            if (!this.isDefaultGraph)
                @case |= 1 << (this.isGKnown ? 3 : 4);
            return this.ClearNewValues(this.Subject == null, this.Predicate==null, this.Object==null, !this.isGKnown, variableBinding, this.listOfFilters.Count>0 ? this.SetVariablesValuesWithFilters(variableBinding, (StoreCallCase)@case) : this.SetVariablesValues(variableBinding, (StoreCallCase)@case));

        }

        void PrepareConstants()
        {
            if (!this.isFirstCall) return;
            this.isFirstCall = false;
            ObjectVariants temp = null;

            if (this.sVariableNode == null)
            {
                var sAsStr = this.Subject as OV_iri;
                if (sAsStr != null)
                {
                    if (!this.store.NodeGenerator.TryGetUri(sAsStr, out temp)) this.isAny = false;
                    this.Subject = temp;
                }
            }

            if (this.pVariableNode == null)
            {
                var predicateAsString = this.Predicate as OV_iri;
                if(predicateAsString!= null)
                {
                        
                    if (!this.store.NodeGenerator.TryGetUri(predicateAsString, out temp)) this.isAny = false;

                    this.Predicate = temp;
                }
            }

            if (this.oVariableNode == null)
            {
                if (this.Object.Variant == ObjectVariantEnum.Iri)
                {
                    if (!this.store.NodeGenerator.TryGetUri((OV_iri)this.Object, out temp)) this.isAny = false;
                    this.Object = temp;
                }
            }

            if (!this.isDefaultGraph && this.graphs.Any())
            {
                // todo filter not existing graphs                                                    
            }
        }

        bool PrepareKnownVarValues()
        { 
                ObjectVariants temp = null;
            if (this.sVariableNode != null && this.Subject!=null && this.Subject.Variant==ObjectVariantEnum.Iri)
                {
                    if (!this.store.NodeGenerator.TryGetUri((OV_iri)this.Subject, out temp))
                    {   
                        return false;
                    }

                    this.Subject = temp;
                }

                if (this.pVariableNode != null && this.Predicate !=null && this.Predicate.Variant==ObjectVariantEnum.Iri)
                {
                    if (!this.store.NodeGenerator.TryGetUri((OV_iri)this.Predicate, out temp))
                    {
                    return false;
                    }

                    this.Predicate = temp;
                }

                if (this.oVariableNode == null && this.Object != null && this.Object.Variant == ObjectVariantEnum.Iri)
                {
                    if (!this.store.NodeGenerator.TryGetUri((OV_iri)this.Object, out temp))
                    {
                        return false;
                    }

                    this.Object = temp;
                }

            return true;
            }
        

        private IEnumerable<SparqlResult> ClearNewValues(bool clearSubject, bool clearPredicate, bool clearObject, bool clearGraph, SparqlResult sourceResult, IEnumerable<SparqlResult> sparqlResults)
        {
            foreach (var result in sparqlResults)
                yield return result;
            if (clearSubject)
                sourceResult[this.sVariableNode] = null;
            if (clearPredicate)
                sourceResult[this.pVariableNode] = null;
            if (clearObject)
                sourceResult[this.oVariableNode] = null;
            if (clearGraph)
                sourceResult[this.variableDataSet.Variable] = null;
        }

        private enum StoreCallCase
        {
            spo = 0, spO = 1, sPo = 2, sPO = 3, Spo = 4, SpO = 5, SPo = 6, SPO = 7,
            gspo = 8, gspO = 9, gsPo = 10, gsPO = 11, gSpo = 12, gSpO = 13, gSPo = 14, gSPO = 15,
            Gspo = 16, GspO = 17, GsPo = 18, GsPO = 19, GSpo = 20, GSpO = 21, GSPo = 22, GSPO = 23,
        }


        private IEnumerable<SparqlResult> SetVariablesValues(SparqlResult variableBinding, StoreCallCase @case)
        {
            switch (@case)
            {
                case StoreCallCase.spo:
                    return this.spo(this.Subject, this.Predicate, this.Object, variableBinding);
                case StoreCallCase.spO:
                    return this.store.GetTriplesWithSubjectPredicate(this.Subject, this.Predicate).Select(o=>variableBinding.Add(o, this.oVariableNode));
                case StoreCallCase.sPo:
                    return this.store.GetTriplesWithSubjectObject(this.Subject, this.Object).Select(p => variableBinding.Add(p, this.pVariableNode));
                    
                case StoreCallCase.sPO:
                    return this.store.GetTriplesWithSubject(this.Subject).Select(t => this.SetValues(variableBinding, t));
                    
                case StoreCallCase.Spo:
                    return this.store.GetTriplesWithPredicateObject(this.Predicate, this.Object).Select(s => variableBinding.Add(s, this.sVariableNode));
                    
                case StoreCallCase.SpO:
                    return this.store.GetTriplesWithPredicate(this.Predicate).Select(t => this.SetValues(variableBinding, t));
                    
                case StoreCallCase.SPo:
                    return this.store.GetTriplesWithObject(this.Object).Select(t => this.SetValues(variableBinding, t));
                    
                case StoreCallCase.SPO:
                    return this.store.GetTriples((s, p, o) => variableBinding.Add(s, this.sVariableNode, p, this.pVariableNode, o, this.oVariableNode));
                    

                case StoreCallCase.gspo:
                    return this.spoGraphs(variableBinding); 
                case StoreCallCase.gspO:
                    return this.graphs.SelectMany(graph => this.store.NamedGraphs
                            .GetObject(this.Subject, this.Predicate, graph)
                            
                            .Select(o => variableBinding.Add(o, this.oVariableNode)));
                case StoreCallCase.gsPo:
                    return this.graphs.SelectMany(graph => this.store.NamedGraphs
                            .GetPredicate(this.Subject, this.Object, graph)
                            
                            .Select(p => variableBinding.Add(p, this.pVariableNode)));
                case StoreCallCase.gsPO:
                    return this.graphs.SelectMany(g => this.store
                        .NamedGraphs
                        .GetTriplesWithSubjectFromGraph(this.Subject, g)
                        
                        .Select(quad => this.SetValues(variableBinding, quad)));
                case StoreCallCase.gSpo:
                    return this.graphs.SelectMany(graph => this.store.NamedGraphs
                            .GetSubject(this.Predicate, this.Object, graph)
                            
                            .Select(s => variableBinding.Add(s, this.sVariableNode)));
                case StoreCallCase.gSpO:
                    return this.graphs.SelectMany(g => this.store
                            .NamedGraphs
                            .GetTriplesWithPredicateFromGraph(this.Predicate, g)
                            
                            .Select(quad => this.SetValues(variableBinding, quad)));
                case StoreCallCase.gSPo:
                    return this.graphs.SelectMany(g => this.store
                            .NamedGraphs
                            .GetTriplesWithObjectFromGraph(this.Object, g)
                            
                            .Select(quad => this.SetValues(variableBinding, quad)));
                case StoreCallCase.gSPO:
                    return this.graphs.SelectMany(g=> this.store.NamedGraphs.GetTriplesFromGraph(g, (s, p, o) => variableBinding.Add(s, this.sVariableNode, p, this.pVariableNode, o, this.oVariableNode))); 

                case StoreCallCase.Gspo:
                    return (this.variableDataSet.Any()
                        ? this.variableDataSet.Where(g => this.store.NamedGraphs.Contains(this.Subject, this.Predicate, this.Object, g))
                        : this.store.NamedGraphs.GetGraph(this.Subject, this.Predicate, this.Object))
                        
                        .Select(g =>variableBinding.Add( g, this.variableDataSet.Variable)); 
                case StoreCallCase.GspO:
                    if(this.variableDataSet.Any())           
                        return this.variableDataSet.SelectMany(g => this.store
                            .NamedGraphs
                            .GetObject(this.Subject, this.Predicate, g)
                        
                            .Select(o =>variableBinding.Add( o, this.oVariableNode, g, this.variableDataSet.Variable)));
                    else return this.store
                        .NamedGraphs
                        .GetTriplesWithSubjectPredicate(this.Subject, this.Predicate)
                        
                            .Select(quad => this.SetValues(variableBinding, quad));

                    // if graphVariable is null, ctor check this.
                case StoreCallCase.GsPo:
                    if (this.variableDataSet.Any())
                        return this.variableDataSet.SelectMany(g => this.store.NamedGraphs.GetPredicate(this.Subject, this.Object, g)
                                                    
                            .Select(p =>variableBinding.Add( p, this.pVariableNode, g, this.variableDataSet.Variable)));
                    else   return this.store
                        .NamedGraphs
                        .GetTriplesWithSubjectObject(this.Subject, this.Object)
                                                
                            .Select(quad => this.SetValues(variableBinding, quad));
                case StoreCallCase.GsPO:
                    if (this.variableDataSet.Any())
                        return this.variableDataSet.SelectMany(g => this.store
                            .NamedGraphs
                            .GetTriplesWithSubjectFromGraph(this.Subject, g)
                                                    
                            .Select(quad => this.SetValues(variableBinding, quad)));
                    else
                        return this.store
                            .NamedGraphs
                            .GetTriplesWithSubject(this.Subject)
                                                    
                            .Select(quad => this.SetValues(variableBinding, quad));
                case StoreCallCase.GSpo:
                    if (this.variableDataSet.Any())
                        return this.variableDataSet.SelectMany(g => this.store
                            .NamedGraphs
                            .GetSubject(this.Predicate, this.Object, g)
                                    
                            .Select(s =>variableBinding.Add( s, this.sVariableNode, g, this.variableDataSet.Variable)));
                    else 
                        return this.store
                            .NamedGraphs
                            .GetTriplesWithPredicateObject(this.Predicate, this.Object)
                            
                            .Select(quad => this.SetValues(variableBinding, quad));
                case StoreCallCase.GSpO:
                    ObjectVariants predicate = this.Predicate;
                    if (this.variableDataSet.Any())
                        return this.variableDataSet.SelectMany(
                                g => this.store.NamedGraphs.GetTriplesWithPredicateFromGraph(predicate, g)
                                    
                                    .Select(quad => this.SetValues(variableBinding, quad)));
                    else
                        return this.store.NamedGraphs.GetTriplesWithPredicate(predicate)
                                                    
                            .Select(quad => this.SetValues(variableBinding, quad));
                case StoreCallCase.GSPo:
                    if (this.variableDataSet.Any())
                        return this.variableDataSet.SelectMany(g => this.store
                            .NamedGraphs
                            .GetTriplesWithObjectFromGraph(this.Object, g))
                            
                            .Select(quad => this.SetValues(variableBinding, quad));
                    return this.store
                        .NamedGraphs
                        .GetTriplesWithObject(this.Object)
                        
                        .Select(quad=> this.SetValues(variableBinding, quad));
                case StoreCallCase.GSPO:
                        if (this.variableDataSet.Any())
                return this.variableDataSet.SelectMany(g => this.store
                        .NamedGraphs
                        .GetTriplesFromGraph(g, (s, p, o) =>
                           variableBinding.Add( s, this.sVariableNode, p, this.pVariableNode, o, this.oVariableNode, g, this.variableDataSet.Variable)));
                        return this.store
                .NamedGraphs
                .GetAll((s, p, o, g) =>
                   variableBinding.Add(s, this.sVariableNode, p, this.pVariableNode, o, this.oVariableNode, g, this.variableDataSet.Variable));
                default:
                    throw new ArgumentOutOfRangeException("case");
            }
        }

        private IEnumerable<SparqlResult> SetVariablesValuesWithFilters(SparqlResult variableBinding, StoreCallCase @case)
        {
            switch (@case)
            {
                case StoreCallCase.spo:
                    return this.spo(this.Subject, this.Predicate, this.Object, variableBinding);
                case StoreCallCase.spO:
                    return this.ApplyFilters(variableBinding, this.oVariableNode, this.store.GetTriplesWithSubjectPredicate(this.Subject, this.Predicate));
                case StoreCallCase.sPo:
                    return this.ApplyFilters(variableBinding, this.pVariableNode, this.store.GetTriplesWithSubjectObject(this.Subject, this.Object));

                case StoreCallCase.sPO:
                    return this.ApplyFilters(variableBinding, this.store.GetTriplesWithSubject(this.Subject));

                case StoreCallCase.Spo:
                    return this.ApplyFilters(variableBinding, this.sVariableNode, this.store.GetTriplesWithPredicateObject(this.Predicate, this.Object));

                case StoreCallCase.SpO:
                    return this.ApplyFilters(variableBinding, this.store.GetTriplesWithPredicate(this.Predicate));

                case StoreCallCase.SPo:
                    return this.ApplyFilters(variableBinding, this.store.GetTriplesWithObject(this.Object));

                case StoreCallCase.SPO:
                    return this.ApplyFilters(variableBinding, this.store.GetTriples((s, p, o) => new TripleOVStruct(s, p, o)));


                case StoreCallCase.gspo:
                    return this.spoGraphs(variableBinding);
                case StoreCallCase.gspO:
                    return this.ApplyFilters(variableBinding, this.oVariableNode, this.graphs.SelectMany(graph => this.store.NamedGraphs
                            .GetObject(this.Subject, this.Predicate, graph)));
                case StoreCallCase.gsPo:
                    return this.ApplyFilters(variableBinding, this.pVariableNode, this.graphs.SelectMany(graph => this.store.NamedGraphs
                            .GetPredicate(this.Subject, this.Object, graph)));
                case StoreCallCase.gsPO:
                    return this.ApplyFilters( variableBinding, this.graphs.SelectMany(g => this.store
                        .NamedGraphs
                        .GetTriplesWithSubjectFromGraph(this.Subject, g)));
                case StoreCallCase.gSpo:
                    return this.ApplyFilters( variableBinding, this.sVariableNode, this.graphs.SelectMany(graph => this.store.NamedGraphs
                            .GetSubject(this.Predicate, this.Object, graph)));
                case StoreCallCase.gSpO:
                    return this.ApplyFilters( variableBinding, this.graphs.SelectMany(g => this.store
                            .NamedGraphs
                            .GetTriplesWithPredicateFromGraph(this.Predicate, g)));
                case StoreCallCase.gSPo:
                    return this.ApplyFilters( variableBinding, this.graphs.SelectMany(g => this.store
                            .NamedGraphs
                            .GetTriplesWithObjectFromGraph(this.Object, g)));
                case StoreCallCase.gSPO:
                    return this.ApplyFilters( variableBinding, this.graphs.SelectMany(g => this.store.NamedGraphs.GetTriplesFromGraph(g, (s, p, o) => new TripleOVStruct(s, p, o))));

                case StoreCallCase.Gspo:
                    return this.ApplyFilters( variableBinding, this.variableDataSet.Variable,
                        this.variableDataSet.Any()
                        ? this.variableDataSet.Where(g => this.store.NamedGraphs.Contains(this.Subject, this.Predicate, this.Object, g))
                        : this.store.NamedGraphs.GetGraph(this.Subject, this.Predicate, this.Object));
                case StoreCallCase.GspO:
                    if (this.variableDataSet.Any())
                        return this.ApplyFilters( variableBinding, this.oVariableNode, this.variableDataSet.SelectMany(g => this.store
                            .NamedGraphs
                            .GetObject(this.Subject, this.Predicate, g)));
                    else return this.ApplyFilters( variableBinding, this.store
                        .NamedGraphs
                        .GetTriplesWithSubjectPredicate(this.Subject, this.Predicate));

                // if graphVariable is null, ctor check this.
                case StoreCallCase.GsPo:
                    if (this.variableDataSet.Any())
                        return this.ApplyFilters( variableBinding, this.pVariableNode, this.variableDataSet.SelectMany(g => this.store.NamedGraphs.GetPredicate(this.Subject, this.Object, g)));
                    else return this.ApplyFilters( variableBinding, this.store
                      .NamedGraphs
                      .GetTriplesWithSubjectObject(this.Subject, this.Object));
                case StoreCallCase.GsPO:
                    if (this.variableDataSet.Any())
                        return this.ApplyFilters( variableBinding, this.variableDataSet.SelectMany(g => this.store
                            .NamedGraphs
                            .GetTriplesWithSubjectFromGraph(this.Subject, g)));
                    else
                        return this.ApplyFilters( variableBinding, this.store
                            .NamedGraphs
                            .GetTriplesWithSubject(this.Subject));
                case StoreCallCase.GSpo:
                    if (this.variableDataSet.Any())
                        return this.ApplyFilters(variableBinding, this.sVariableNode, this.variableDataSet.SelectMany(g => this.store
                            .NamedGraphs
                            .GetSubject(this.Predicate, this.Object, g)));
                    else
                        return this.ApplyFilters( variableBinding, this.store
                            .NamedGraphs
                            .GetTriplesWithPredicateObject(this.Predicate, this.Object));
                case StoreCallCase.GSpO:
                    if (this.variableDataSet.Any())
                        return this.ApplyFilters( variableBinding, this.variableDataSet.SelectMany(g => this.store.NamedGraphs.GetTriplesWithPredicateFromGraph(this.Predicate, g)));
                    else
                        return this.ApplyFilters( variableBinding, this.store.NamedGraphs.GetTriplesWithPredicate(this.Predicate));
                case StoreCallCase.GSPo:
                    if (this.variableDataSet.Any())
                        return this.ApplyFilters( variableBinding, this.variableDataSet.SelectMany(g => this.store
                            .NamedGraphs
                            .GetTriplesWithObjectFromGraph(this.Object, g)));
                    return this.ApplyFilters( variableBinding, this.store
                        .NamedGraphs
                        .GetTriplesWithObject(this.Object));
                case StoreCallCase.GSPO:
                    if (this.variableDataSet.Any())
                        return this.ApplyFilters( variableBinding, this.variableDataSet.SelectMany(g => this.store
                                .NamedGraphs
                                .GetTriplesFromGraph(g, (s, p, o) => new QuadOVStruct(s, p, o, g))));
                    return this.ApplyFilters( variableBinding, this.store
                        .NamedGraphs
                        .GetAll((s, p, o, g) => 
                          new QuadOVStruct(s, p, o, g)));
                default:
                    throw new ArgumentOutOfRangeException("case");
            }
        }

        public void Substitution(SparqlResult variableBinding, Action<ObjectVariants, ObjectVariants, ObjectVariants> actTriple)
        {
            var subject = this.sVariableNode is IBlankNode
                 ? this.store.NodeGenerator.CreateBlankNode(((IBlankNode)this.sVariableNode).Name+variableBinding.Id)
                 : (this.sVariableNode != null ? variableBinding[this.sVariableNode] : this.Subject);

            var predicate = this.pVariableNode != null ? variableBinding[this.pVariableNode] : this.Predicate;

            var @object = this.oVariableNode is IBlankNode
                 ? this.store.NodeGenerator.CreateBlankNode(((IBlankNode)this.oVariableNode).Name+variableBinding.Id)
                 : (this.oVariableNode != null ? variableBinding[this.oVariableNode] : this.Object);
            actTriple(subject, predicate, @object);
        }

        public void Substitution(SparqlResult variableBinding, ObjectVariants g, Action<ObjectVariants, ObjectVariants, ObjectVariants, ObjectVariants> actQuard)
        {
            var subject = this.sVariableNode is IBlankNode
                ? this.store.NodeGenerator.CreateBlankNode(((IBlankNode)this.sVariableNode).Name + variableBinding.Id, ((IIriNode)g).UriString)
                : (this.sVariableNode != null ? variableBinding[this.sVariableNode] : this.Subject);
            var predicate = this.pVariableNode != null ? variableBinding[this.pVariableNode] : this.Predicate;
            var @object = this.Object == null && this.oVariableNode is IBlankNode
                  ? this.store.NodeGenerator.CreateBlankNode(((IBlankNode)this.oVariableNode).Name + variableBinding.Id, ((IIriNode)g).UriString)
                  : (this.oVariableNode != null ? variableBinding[this.oVariableNode] : this.Object);

            actQuard(g, subject, predicate, @object);
        }

        public void Substitution(SparqlResult variableBinding, VariableNode gVariableNode,
            Action<ObjectVariants, ObjectVariants, ObjectVariants, ObjectVariants> actQuard)
        {
            ObjectVariants g;
            g = variableBinding[gVariableNode];
            if (g == null)
                throw new Exception("graph hasn't value");

            this.Substitution(variableBinding, g, actQuard);
        }

        public void AddFilter(SparqlFilter node)
        {
            this.listOfFilters.Add(node);
        }

        public IEnumerable<SparqlResult> ApplyFilters(SparqlResult variablesBindings, VariableNode variable,
   IEnumerable<ObjectVariants> baseStoreCall)
        {
            return this.listOfFilters.Aggregate(baseStoreCall
                .Select(node => new KeyValuePair<ObjectVariants, SparqlResult>(node, variablesBindings.Add(node, variable))),
                (current, sparqlFilter) => current.Where(valueAndResult => sparqlFilter.SparqlExpression.Test(valueAndResult.Value)))
                        .Select(pair => pair.Key)
                        .Select(node => variablesBindings.Add(node, variable));
        }

        public IEnumerable<SparqlResult> ApplyFilters(SparqlResult variablesBindings, IEnumerable<TripleOVStruct> baseStoreCall)
        {
            return this.listOfFilters.Aggregate(baseStoreCall
                .Select(t => new KeyValuePair<TripleOVStruct, SparqlResult>(t, this.SetValues(variablesBindings, t))),
                (current, sparqlFilter) =>
                    current.Where(valueAndResult => sparqlFilter.SparqlExpression.Test(valueAndResult.Value)))
                .Select(pair => pair.Key)
                .ToArray()
                .Select(t => this.SetValues(variablesBindings, t));
        }

        public IEnumerable<SparqlResult> ApplyFilters(SparqlResult variablesBindings, IEnumerable<QuadOVStruct> baseStoreCall)
        {
            return this.listOfFilters.Aggregate(baseStoreCall
                .Select(t => new KeyValuePair<QuadOVStruct, SparqlResult>(t, this.SetValues(variablesBindings, t))),
                (current, sparqlFilter) =>
                    current.Where(valueAndResult => sparqlFilter.SparqlExpression.Test(valueAndResult.Value)))
                .Select(pair => pair.Key)
                .ToArray()
                .Select(t => this.SetValues(variablesBindings, t));
        }

        private SparqlResult SetValues(SparqlResult result, TripleOVStruct triple)
        {
            if (this.Subject == null)
            {
                result.Add(triple.Subject, this.sVariableNode);
            }

            if (this.Predicate == null)
            {
                result.Add(triple.Predicate, this.pVariableNode);
            }

            if (this.Object == null)
            {
                result.Add(triple.Object, this.oVariableNode);
            }

            return result;
        }

        private SparqlResult SetValues(SparqlResult result, QuadOVStruct quad)
        {
            if (this.Subject == null)
            {
                result.Add(quad.Subject, this.sVariableNode);
            }

            if (this.Predicate == null)
            {
                result.Add(quad.Predicate, this.pVariableNode);
            }

            if (this.Object == null)
            {
                result.Add(quad.Object, this.oVariableNode);
            }

            if (!this.isGKnown)
            {
                result.Add(quad.Graph, this.variableDataSet.Variable);
            }

            return result;
        }

     

        private IEnumerable<SparqlResult> spoGraphs(SparqlResult variablesBindings)
        {
            if (this.graphs.Any(g => this.store.NamedGraphs.Contains(this.Subject, this.Predicate, this.Object, g)))
                yield return variablesBindings;
        }

        private IEnumerable<SparqlResult> spo(ObjectVariants subjectNode, ObjectVariants predicateNode, ObjectVariants objNode, SparqlResult variablesBindings)
        {
            if (this.store.Contains(subjectNode, predicateNode, objNode))
                yield return variablesBindings;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            
            switch (reader.GetAttribute("graph"))
            {
                case "variable":
                    this.variableDataSet = new VariableDataSet();
                    this.variableDataSet.ReadXml(reader);
                    this.isDefaultGraph = false;
                    break;
                case "named":
                    this.graphs=new DataSet();
                    this.graphs.ReadXml(reader);
                    this.isDefaultGraph = false;
                    break;
                case "default":
                    this.isDefaultGraph = true;
                    break;
                default: throw new Exception();
            }
            this.Subject=(ObjectVariants) Query.SparqlQuery.CreateByTypeAttribute(reader);
            this.Predicate = (ObjectVariants)Query.SparqlQuery.CreateByTypeAttribute(reader);
            this.Object = (ObjectVariants)Query.SparqlQuery.CreateByTypeAttribute(reader);

            this.sVariableNode = Subject as VariableNode;
            this.pVariableNode = Predicate as VariableNode;
            this.oVariableNode = Object as VariableNode;
        }

    public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("triple");

            writer.WriteAttributeString("type", this.GetType().ToString());

            if (!this.isDefaultGraph)
            {
                if (this.variableDataSet != null)
                {
                    writer.WriteAttributeString("graph", "variable");
                    this.variableDataSet.WriteXml(writer);
                }
                else
                {
                    writer.WriteAttributeString("graph", "named");
                    this.graphs.WriteXml(writer);
                }
            }
            else
            {
                writer.WriteAttributeString("graph", "default");
            }


            IXmlSerializable s = this.sVariableNode ?? this.Subject;
            s.WriteXml(writer);
            IXmlSerializable p = this.pVariableNode ?? this.Predicate;
            p.WriteXml(writer);
            IXmlSerializable o = this.oVariableNode ?? this.Object;
            o.WriteXml(writer);
            
            
            writer.WriteEndElement();
        }
    }
}