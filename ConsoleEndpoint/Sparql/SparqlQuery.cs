using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SparqlQuery.SparqlClasses.GraphPattern;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;
using SparqlQuery.SparqlClasses.SolutionModifier;

namespace SparqlQuery.SparqlClasses.Query
{
    [XmlInclude(typeof(SparqlSelectQuery))]
    [XmlInclude(typeof(VariableNode))]
    [Serializable]
    public class SparqlQuery :IXmlSerializable
    {
        protected internal ISparqlGraphPattern sparqlWhere;

        public SparqlSolutionModifier SparqlSolutionModifier;

        public ISparqlGraphPattern valueDataBlock;

        public readonly SparqlResultSet ResultSet;

        [NonSerialized]
        protected RdfQuery11Translator q;

        protected IEnumerable<SparqlResult> Seed;

        /// <summary>
        /// 4 serialization only 
        /// </summary>
        public SparqlQuery()
        {
        }

        public SparqlQuery(RdfQuery11Translator q)
        {
            this.q = q;
            this.ResultSet = new SparqlResultSet(this, q.prolog);
        }

        public virtual SparqlResultSet Run()
        {
            this.ResultSet.Variables = this.q.Variables;
            this.Seed = Enumerable.Repeat(new SparqlResult(this.q), 1);
            this.ResultSet.Results = this.Seed;
            if (this.valueDataBlock != null) this.ResultSet.Results = this.valueDataBlock.Run(this.ResultSet.Results);
            this.ResultSet.Results = this.sparqlWhere.Run(this.ResultSet.Results);

            if (this.SparqlSolutionModifier != null) this.ResultSet.Results = this.SparqlSolutionModifier.Run(this.ResultSet.Results, this.ResultSet);
            return this.ResultSet;
        }

        // public SparqlQueryTypeEnum Type { get; set; }
        /*
                public abstract SparqlQueryTypeEnum QueryType { get; }
        */
        internal void SetValues(ISparqlGraphPattern valueDataBlock)
        {
            this.valueDataBlock = valueDataBlock;
        }

        public void Create(ISparqlGraphPattern sparqlWhere, SparqlSolutionModifier sparqlSolutionModifier1)
        {
            this.sparqlWhere = sparqlWhere;
            this.SparqlSolutionModifier = sparqlSolutionModifier1;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            this.sparqlWhere = (ISparqlGraphPattern)CreateByTypeAttribute(reader);
            this.SparqlSolutionModifier = new SparqlSolutionModifier();
            SparqlSolutionModifier.ReadXml(reader);
            if (reader.IsStartElement("inlineOneVariable") || reader.IsStartElement("inline")) this.valueDataBlock = (ISparqlGraphPattern)CreateByTypeAttribute(reader);
        }

        public static IXmlSerializable CreateByTypeAttribute(XmlReader reader)
        {
          
            reader.ReadStartElement();

            if (!reader.IsStartElement())
                return null;
            //reader.ReadStartElement();
           // reader.MoveToAttribute("type");
           // if (!reader.ReadAttributeValue()) { }
            
            string typeString = reader.GetAttribute("type");
            if (typeString == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                Type type = Type.GetType(typeString);

                IXmlSerializable obj = (IXmlSerializable)type.GetConstructor(new Type[0]).Invoke(new object[0]);
                obj.ReadXml(reader);
                if (!reader.IsEmptyElement)
                reader.ReadEndElement();
            //reader.Skip();
                return obj;
            }
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", this.GetType().ToString());

            this.sparqlWhere.WriteXml(writer);

            this.SparqlSolutionModifier.WriteXml(writer);
            this.valueDataBlock?.WriteXml(writer);
        }
    }
}
