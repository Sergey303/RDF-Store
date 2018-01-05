using System;
using System.Xml;
using RDFCommon.OVns;
using RDFCommon.OVns.date;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlTimeZone : SparqlUnaryExpression<OV_dayTimeDuration>
    {
        private SparqlExpression sparqlExpression;

        public SparqlTimeZone(SparqlExpression value)
            :base(f =>
            {
                if (f is DateTime)
                    return TimeZoneInfo.Utc.GetUtcOffset((DateTime)f);
                else if (f is DateTimeOffset)
                    return ((DateTimeOffset)f).Offset;
                throw new ArgumentException();
            }, value, f=>new OV_dayTimeDuration(f))
        {  
          
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("timeZone");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.sparqlExpression.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
