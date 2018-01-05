using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using SparqlQuery.SparqlClasses.Expressions;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    public class SparqlFilter : ISparqlGraphPattern
    {
        /// <summary>
        /// 4 serialization only
        /// </summary>
        public SparqlFilter()
        {
        }

        // public Func<IEnumerable<Action>> SelectVariableValuesOrFilter { get; set; }
        // public SparqlResultSet resultSet;
        // private SparqlConstraint sparqlConstraint;
        public SparqlExpression SparqlExpression;

        public SparqlFilter(SparqlExpression sparqlExpression)
        {
            // TODO: Complete member initialization
            this.SparqlExpression = sparqlExpression;
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            return variableBindings.Where(this.SparqlExpression.Test);
        }

        public SparqlGraphPatternType PatternType
        {
            get
            {
                return SparqlGraphPatternType.Filter;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {

            this.SparqlExpression = (SparqlExpression)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader);

        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Filter");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.SparqlExpression.WriteXml(writer);
            writer.WriteEndElement();
        }
       
    }
}