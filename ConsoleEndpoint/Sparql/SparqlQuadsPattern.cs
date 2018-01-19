using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.GraphPattern
{
    [Serializable]
    public class SparqlQuadsPattern : List<ISparqlGraphPattern>, ISparqlGraphPattern
    {
        public virtual IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings)
        {
            return this.Aggregate(variableBindings, (current, pattern) => pattern.Run(current));
        }

        public virtual SparqlGraphPatternType PatternType { get{return SparqlGraphPatternType.ListOfPatterns;} }
        public virtual XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
           
            while (reader.IsStartElement())
            {
                this.Add((ISparqlGraphPattern)SparqlQuery.SparqlClasses.Query.SparqlQuery.CreateByTypeAttribute(reader));
            }


        }

        public virtual void WriteXml(XmlWriter writer)
        {
            foreach (ISparqlGraphPattern pattern in this)
            {
              pattern.WriteXml(writer);
            }
        }
    }
  
}
