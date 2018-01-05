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
    public class SparqlMayBeOneTriple:  ISparqlGraphPattern
    {
        private IEnumerable<ISparqlGraphPattern> triples;
        private ObjectVariants sNode;
        private ObjectVariants oNode;               
        private readonly RdfQuery11Translator q;

        public SparqlMayBeOneTriple(IEnumerable<ISparqlGraphPattern> triples, ObjectVariants s, ObjectVariants o, RdfQuery11Translator q)
        {
            this.oNode = o;
            this.q = q;
            this.triples = triples;
            this.sNode = s;
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            var firstVar = this.sNode as VariableNode;
            var secondVar = this.oNode as VariableNode;

            foreach (var variableBinding in variableBindings)
            {
                ObjectVariants s = firstVar == null ? this.sNode : variableBinding[firstVar];
                ObjectVariants o = secondVar == null ? this.oNode : variableBinding[secondVar];

                switch (NullablePairExt.Get(s, o))
                {
                    case NP.bothNull:
                        foreach (var subjectNode in this.q.Store.GetAllSubjects())
                        {
                            yield return variableBinding.Add(subjectNode, firstVar, subjectNode, secondVar);
                            variableBinding[secondVar] = null;
                            foreach (
                                var tr in
                                this.triples.Aggregate(
                                    Enumerable.Repeat(variableBinding, 1),
                                    (enumerable, triple) => triple.Run(enumerable))) yield return tr;
                        }

                        continue;
                    case NP.leftNull:
                        yield return variableBinding.Add(o, firstVar);
                        variableBinding[firstVar] = null;
                        break;
                    case NP.rigthNull:
                        yield return variableBinding.Add(s, secondVar);
                        variableBinding[secondVar] = null;
                        break;
                    case NP.bothNotNull:
                        if (s.Equals(o)) yield return variableBinding;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // todo optimize
                foreach (
                    var tr in
                    this.triples.Aggregate(
                        Enumerable.Repeat(variableBinding, 1),
                        (enumerable, triple) => triple.Run(enumerable))) yield return tr;
            }
        }

        public SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.SparqlTriple;} }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this.oNode = (ObjectVariants)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader);
            this.sNode = (ObjectVariants)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader);
            var triplesTmp = new List<ISparqlGraphPattern>();
            while (reader.IsStartElement())
            {
                triplesTmp.Add((ISparqlGraphPattern)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader));
            }
            this.triples = triplesTmp;
        }

        public  void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("mayBeOne");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.oNode.WriteXml(writer);
            this.sNode.WriteXml(writer);
            foreach (var triple in this.triples)
            {
                triple.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
    }
}