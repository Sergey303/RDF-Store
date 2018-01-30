using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleEndpoint.XML_Serialization
{
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml.Linq;

    using ConsoleEndpoint.Sparql_adapters;

    using SparqlQuery.SparqlClasses.GraphPattern;
    using SparqlQuery.SparqlClasses.Query;

    public static class XMLSparqlSerialize
    {
       public static SparqlQuery Serialize(this XElement xQuery)
        {
            switch (xQuery.Name.ToString())
            {
                case "SparqlSelectQuery":
                    return new SparqlSelectQuery(xQuery.Elements().Select(SerializeNode).ToArray());
            }
            throw new ArgumentException("xQuery");
        }

        private static ISparqlGraphPattern SerializeNode(XElement xNode)
        {
            switch (xNode.Name.ToString())
            {
                case "triple":
                    return new SparqlTriple(sVarName:xNode.Attribute("sv")?.Value,
                        pVarName: xNode.Attribute("pv")?.Value,
                        oVarName: xNode.Attribute("ov")?.Value,
                        subjectSource: xNode.Attribute("s")?.Value,
                        predicateSource: xNode.Attribute("p")?.Value,
                        o: new object []{ SerializeOVT(xNode), xNode.Attribute("o")?.Value });
            }
            throw new ArgumentException(xNode.ToString());
        }

        private static int SerializeOVT(XElement xNode)
        {
            Contract.Requires(xNode.Attribute("otype") != null);
            OVT vid = OVT.iri;
            Contract.Requires(OVT.TryParse(xNode.Attribute("otype").Value, out vid));
            return (int)vid;
        }
    }
}
