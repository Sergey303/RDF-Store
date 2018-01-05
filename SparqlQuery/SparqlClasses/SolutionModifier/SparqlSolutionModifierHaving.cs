using System;
using System.Collections.Generic;
using System.Linq;
using SparqlQuery.SparqlClasses.Expressions;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SolutionModifier
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    public class SparqlSolutionModifierHaving : List<SparqlExpression>, IXmlSerializable
    {
        private readonly string name = "having";

        internal IEnumerable<SparqlResult> Having(IEnumerable<SparqlResult> resultsGroups, RdfQuery11Translator q)
        {
            // IEnumerable<SparqlResult> resultsGroups = enumerable as SparqlResult[] ?? enumerable.ToArray();
            // if (!resultsGroups.Any()) return resultsGroups; //todo
            return this.Aggregate(resultsGroups, (result, testExpr) =>
            {
                switch (testExpr.AggregateLevel)
                {
                    case SparqlExpression.VariableDependenceGroupLevel.Const:
                        return (bool) testExpr.Const.Content ? result : Enumerable.Empty<SparqlResult>();
                    case SparqlExpression.VariableDependenceGroupLevel.UndependableFunc:
                        return testExpr.Test(null) ? result : Enumerable.Empty<SparqlResult>();
                    case SparqlExpression.VariableDependenceGroupLevel.SimpleVariable:
                        return result.Where(testExpr.Test);
                    case SparqlExpression.VariableDependenceGroupLevel.Group:
                        var sparqlGroupOfResults = new SparqlGroupOfResults(q) {Group = result.Select(sparqlResult => sparqlResult.Clone()).ToArray() };
                        return testExpr.Test(sparqlGroupOfResults) ? sparqlGroupOfResults.Group : Enumerable.Empty<SparqlResult>();
                    case SparqlExpression.VariableDependenceGroupLevel.GroupOfGroups:
                    default:
                        throw new Exception("requested grouping");
                }
            });
        }

        internal IEnumerable<SparqlGroupOfResults> Having4CollectionGroups(
            IEnumerable<SparqlGroupOfResults> resultsGroups, RdfQuery11Translator q)
        {
            return this.Aggregate(resultsGroups, (result, testExpr) =>
            {
                switch (testExpr.AggregateLevel)
                {
                    case SparqlExpression.VariableDependenceGroupLevel.Const:
                        if ((bool) testExpr.Const.Content) return result;
                        else return Enumerable.Empty<SparqlGroupOfResults>();
                    case SparqlExpression.VariableDependenceGroupLevel.UndependableFunc:
                        if ((bool) testExpr.Operator(null)) return result;
                        else return Enumerable.Empty<SparqlGroupOfResults>();
                    case SparqlExpression.VariableDependenceGroupLevel.SimpleVariable:
                    case SparqlExpression.VariableDependenceGroupLevel.Group:
                        return result.Where(testExpr.Test);
                    case SparqlExpression.VariableDependenceGroupLevel.GroupOfGroups:

                        var group = new SparqlGroupOfResults(q)
                        {
                            Group = result.Select(sparqlResult => sparqlResult.Clone()).ToArray()
                        };
                        return testExpr.Test(group) ? group.Group.Cast<SparqlGroupOfResults>() : Enumerable.Empty<SparqlGroupOfResults>(); 
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return resultsGroups;
            });
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
           
            while (reader.IsStartElement())
            {
                this.Add((SparqlExpression)new XmlSerializer(typeof(SparqlExpression)).Deserialize(reader));
          
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(this.name);
            writer.WriteAttributeString("type", this.GetType().ToString());

            foreach (var expr in this)
            {
                expr.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
    }
}

