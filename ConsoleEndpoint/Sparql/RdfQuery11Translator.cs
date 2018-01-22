using System;
using System.Collections.Generic;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;

namespace SparqlQuery
{
    [Serializable]
    public class RdfQuery11Translator
    {
        public Dictionary<string, VariableNode> Variables = new Dictionary<string, VariableNode>();

        // public Dictionary<string, SparqlBlankNode> BlankNodes = new Dictionary<string, SparqlBlankNode>();
     //   public DataSet ActiveGraphs = new DataSet();

     //   public DataSet NamedGraphs = new DataSet();

        public readonly Prologue prolog = new Prologue();

        // public IUriNode With;
        public IStore Store
        {
            get
            {
                return this.store;
            }

            set
            {
                this.store = value;

                // StoreCalls = new SparqlTripletsStoreCalls(Store);                
            }
        }

        // public SparqlTripletsStoreCalls StoreCalls;
        private IStore store;

        public RdfQuery11Translator(IStore store1)
        {
            this.Store = store1;
        }

        internal VariableNode GetVariable(string p)
        {
            VariableNode variable;
            if (this.Variables.TryGetValue(p, out variable)) return (VariableNode)variable;
            this.Variables.Add(p, variable = new VariableNode(p, this.Variables.Count));
            return (VariableNode)variable;
        }

        // internal IEnumerable<VariableNode> GetVariables(int p)
        // {
        // return Variables.Values.Cast<>.Skip(p);
        // }
        //internal SparqlExpressionAsVariable CreateExpressionAsVariable(
        //    VariableNode variableNode,
        //    SparqlExpression sparqlExpression)
        //{
        //    return new SparqlExpressionAsVariable(variableNode, sparqlExpression, this);
        //}

        //internal DataSet SetNamedGraphOrVariable(ObjectVariants sparqlNode, DataSet namedDataSet)
        //{
        //    VariableNode graphVariable = sparqlNode as VariableNode;
        //    return graphVariable != null
        //               ? new VariableDataSet(graphVariable, namedDataSet)
        //               : new DataSet() { sparqlNode };
        //}

        //public new SparqlBlankNode CreateBlankNode(string blankNodeString)
        //{
        //    VariableNode blankNode;
        //    if (this.Variables.TryGetValue(blankNodeString, out blankNode)) return (SparqlBlankNode)blankNode;
        //    this.Variables.Add(blankNodeString, blankNode = new SparqlBlankNode(blankNodeString, this.Variables.Count));
        //    return (SparqlBlankNode)blankNode;
        //}

        //public new SparqlBlankNode CreateBlankNode()
        //{
        //    var blankNode = new SparqlBlankNode(this.store.NodeGenerator.CreateBlank(), this.Variables.Count);
        //    this.Variables.Add(Guid.NewGuid().ToString(), blankNode); // todo

        //    return blankNode;
        //}
    }
}
