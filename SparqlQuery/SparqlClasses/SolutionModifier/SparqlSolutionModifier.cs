using System;
using System.Collections.Generic;
using System.Linq;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SolutionModifier
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    [Serializable]
    public class SparqlSolutionModifier :IXmlSerializable
    {
        public SparqlSolutionModifier()
        {
            // if (isDistinct)
            // Modifier = enumerable => enumerable.Distinct();
        }
        private RdfQuery11Translator q;

        private SparqlSolutionModifierLimit LimitOffset;
        
        private SparqlSolutionModifierGroup Group;
        public SparqlSelect Select;
        private SparqlSolutionModifierHaving sparqlSolutionModifierHaving;
        private SparqlSolutionModifierOrder sparqlSolutionModifierOrder;

        internal void Add(SparqlSolutionModifierLimit sparqlSolutionModifierLimit)
        {
            this.LimitOffset = sparqlSolutionModifierLimit;
        }

        internal void Add(SparqlSolutionModifierOrder sparqlSolutionModifierOrder)
        {
            this.sparqlSolutionModifierOrder = sparqlSolutionModifierOrder;
        }

        internal void Add(SparqlSolutionModifierHaving sparqlSolutionModifierHaving, RdfQuery11Translator q)
        {
            this.sparqlSolutionModifierHaving = sparqlSolutionModifierHaving;
            this.q = q;
        }

        internal void Add(SparqlSolutionModifierGroup sparqlSolutionModifierGroup)
        {
            this.Group = sparqlSolutionModifierGroup;
        }

        internal void Add(SparqlSelect projection)
        {
            this.Select = projection;
        }

        public IEnumerable<SparqlResult> Run( IEnumerable<SparqlResult> results, SparqlResultSet sparqlResultSet)
        {
            if (this.Group != null)
            {
                var groupedResults = this.Group.Group(results.Select(r => r.Clone()));
                if (this.sparqlSolutionModifierHaving != null)
                    groupedResults= this.sparqlSolutionModifierHaving.Having4CollectionGroups(groupedResults, this.q);

                if (this.sparqlSolutionModifierOrder != null)
                    groupedResults= this.sparqlSolutionModifierOrder.Order4Grouped(groupedResults).Cast<SparqlGroupOfResults>();

                var res = groupedResults.Cast<SparqlResult>();
                if (this.Select != null)
                    res = this.Select.Run(res, sparqlResultSet, true);

                if (this.LimitOffset != null)
                    res = this.LimitOffset.LimitOffset(res);
                return res;
            }
            else
            {
                if (this.sparqlSolutionModifierHaving != null)
                    results = this.sparqlSolutionModifierHaving.Having(results, this.q);

                if (this.sparqlSolutionModifierOrder != null)
                    results = this.sparqlSolutionModifierOrder.Order(results.Select(r => r.Clone()));

                if (this.Select != null)
                    results = this.Select.Run(results, sparqlResultSet, false);

                if (this.LimitOffset != null)
                    results = this.LimitOffset.LimitOffset(results);
                return results;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var modify = Query.SparqlQuery.CreateByTypeAttribute(reader);
            
            this.LimitOffset=modify as SparqlSolutionModifierLimit;

            this.Group = modify as SparqlSolutionModifierGroup;
            this.Select = modify as SparqlSelect;
            this.sparqlSolutionModifierHaving = modify as SparqlSolutionModifierHaving;
            this.sparqlSolutionModifierOrder = modify as SparqlSolutionModifierOrder;
        }

        public void WriteXml(XmlWriter writer)
        {
            this.Select?.WriteXml(writer);
            sparqlSolutionModifierHaving?.WriteXml(writer);
        sparqlSolutionModifierOrder?.WriteXml(writer);
        LimitOffset?.WriteXml(writer);
        Group?.WriteXml(writer);

    }
}
}
