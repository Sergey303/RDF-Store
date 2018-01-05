using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.SolutionModifier
{
    public class SparqlSolutionModifierLimit: IXmlSerializable

    {
    public Func<IEnumerable<SparqlResult>, IEnumerable<SparqlResult>> LimitOffset;
    private int limit;

    private int offset;

    // public bool IsOffsetFirst
    internal void CreateLimit(int p)
    {
        this.limit = p;
        if (this.LimitOffset == null) this.LimitOffset = enumerable => enumerable.Take(this.limit);
        else
        {
            var limitOffsetClone =
                this.LimitOffset.Clone() as Func<IEnumerable<SparqlResult>, IEnumerable<SparqlResult>>;
            this.LimitOffset = enumerable => limitOffsetClone(enumerable).Take(this.limit);
        }
    }

    internal void CreateOffset(int p)
    {
        this.offset = p;
        if (this.LimitOffset == null) this.LimitOffset = enumerable => enumerable.Skip(this.offset);
        else
        {
            var limitOffsetClone =
                this.LimitOffset.Clone() as Func<IEnumerable<SparqlResult>, IEnumerable<SparqlResult>>;
            this.LimitOffset = enumerable => limitOffsetClone(enumerable).Skip(this.offset);
        }
    }

    // internal void CreateDistinct()
    // {
    // HashSet<SparqlResult> results = new HashSet<SparqlResult>();
    // if (offset == -1)
    // //limit>0
    // LimitOffset = enumerable =>
    // {
    // foreach (var res in enumerable)
    // {
    // if (results.Contains(res)) continue;
    // results.Add(res);
    // if (results.Count == limit) break;
    // }
    // return results;
    // };
    // else if (limit == -1)
    // //offset>0
    // LimitOffset = enumerable => enumerable.Distinct<SparqlResult>().Skip(offset);
    // else LimitOffset = enumerable =>
    // {
    // foreach (var res in enumerable)
    // {
    // if (results.Contains(res)) continue;
    // results.Add(res);
    // if (results.Count == offset + limit) break;
    // }
    // return results.Skip(offset);
    // };
    // }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            limit = int.Parse(reader.GetAttribute("limit"));
            offset = int.Parse(reader.GetAttribute("offset"));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("limintOfset");
            writer.WriteAttributeString("limit",limit.ToString());
            writer.WriteAttributeString("offset", offset.ToString());
            writer.WriteEndElement();
        }
    }
}
