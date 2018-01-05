using System.Collections.Generic;
using RDFCommon.OVns;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern.Triples
{
    public interface ISparqlTripletsStoreCalls
    {
        IEnumerable<SparqlResult> spO(ObjectVariants subjNode, ObjectVariants predicateNode,
            VariableNode obj, SparqlResult variablesBindings);

        IEnumerable<SparqlResult> spOVarGraphs(ObjectVariants subjNode, ObjectVariants predicateNode,
            VariableNode obj, SparqlResult variablesBindings, VariableDataSet variableDataSet);

        IEnumerable<SparqlResult> spOGraphs(ObjectVariants subjNode, ObjectVariants predicateNode,
            VariableNode obj, SparqlResult variablesBindings, DataSet graphs);

        IEnumerable<SparqlResult> Spo(VariableNode subj, ObjectVariants predicateNode, ObjectVariants objectNode,
            SparqlResult variablesBindings);

        IEnumerable<SparqlResult> SpoGraphs(VariableNode subj, ObjectVariants predicateNode,
            ObjectVariants objectNode, SparqlResult variablesBindings, DataSet graphs);

        IEnumerable<SparqlResult> SpoVarGraphs(VariableNode subj, ObjectVariants predicateNode,
            ObjectVariants objectNode, SparqlResult variablesBindings, VariableDataSet variableDataSet);

        IEnumerable<SparqlResult> SpO(VariableNode s, ObjectVariants predicate, VariableNode o, SparqlResult variablesBindings);

        IEnumerable<SparqlResult> SpOGraphs(VariableNode s, ObjectVariants predicate, VariableNode o,
            SparqlResult variablesBindings, DataSet graphs);

        IEnumerable<SparqlResult> SpOVarGraphs(VariableNode s, ObjectVariants predicate, VariableNode o,
            SparqlResult variablesBindings, VariableDataSet graphs);

        IEnumerable<SparqlResult> sPo(ObjectVariants subj, VariableNode pred, ObjectVariants obj, SparqlResult variablesBindings);

        IEnumerable<SparqlResult> sPoGraphs(ObjectVariants subj, VariableNode pred, ObjectVariants obj,
            SparqlResult variablesBindings, DataSet graphs);

        IEnumerable<SparqlResult> sPoVarGraphs(ObjectVariants subj, VariableNode pred, ObjectVariants obj,
            SparqlResult variablesBindings, VariableDataSet variableDataSet);

        IEnumerable<SparqlResult> sPO(ObjectVariants subj, VariableNode pred, VariableNode obj,
            SparqlResult variablesBindings);

        IEnumerable<SparqlResult> sPOGraphs(ObjectVariants subj, VariableNode pred,
            VariableNode obj, SparqlResult variablesBindings, DataSet graphs);

        IEnumerable<SparqlResult> sPOVarGraphs(ObjectVariants subj, VariableNode pred,
            VariableNode obj, SparqlResult variablesBindings, VariableDataSet variableDataSet);

        IEnumerable<SparqlResult> SPo(VariableNode subj, VariableNode predicate, ObjectVariants obj,
            SparqlResult variablesBindings);

        IEnumerable<SparqlResult> SPoGraphs(VariableNode subj, VariableNode pred,
            ObjectVariants obj, SparqlResult variablesBindings, DataSet graphs);

        IEnumerable<SparqlResult> SPoVarGraphs(VariableNode subj, VariableNode pred,
            ObjectVariants obj, SparqlResult variablesBindings, VariableDataSet variableDataSet);

        IEnumerable<SparqlResult> SPO(VariableNode subj, VariableNode predicate, VariableNode obj,
            SparqlResult variablesBindings);

        IEnumerable<SparqlResult> SPOGraphs(VariableNode subj, VariableNode predicate, VariableNode obj,
            SparqlResult variablesBindings, DataSet graphs);

        IEnumerable<SparqlResult> SPOVarGraphs(VariableNode subj, VariableNode predicate, VariableNode obj,
            SparqlResult variablesBindings, VariableDataSet variableDataSet);

        IEnumerable<SparqlResult> spoGraphs(ObjectVariants subjectNode, ObjectVariants predicateNode, ObjectVariants objectNode, SparqlResult variablesBindings, DataSet graphs);
        IEnumerable<SparqlResult> spoVarGraphs(ObjectVariants subjectNode, ObjectVariants predicateNode, ObjectVariants objectNode, SparqlResult variablesBindings, VariableDataSet graphs);

        IEnumerable<SparqlResult> spo(ObjectVariants subjectNode, ObjectVariants predicateNode, ObjectVariants objNode, SparqlResult variablesBindings);
    }
}