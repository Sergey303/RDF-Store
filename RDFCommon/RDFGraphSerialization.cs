using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using RDFCommon.OVns.numeric;

namespace RDFCommon
{
    public static class RdfGraphSerialization
    {
        public static XElement ToXml(this IGraph g)
        {
            XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
            var prefixes = new Dictionary<string, string>();                    
                
            var RDF = new XElement(rdf + "RDF", new XAttribute(XNamespace.Xmlns + "rdf", rdf));
            int i = 0;
            foreach (var s in g.GetAllSubjects())
            {
                XAttribute id = null;
                if (s is IIriNode)
                {
                    id = new XAttribute(rdf + "about", ((ObjectVariants)s).Content);
                }
                else if (s is IBlankNode)
                {
                    id = new XAttribute(rdf + "nodeID", ((IBlankNode)s).Name);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
                RDF.Add(new XElement(rdf + "Description", id,
                    g.GetTriplesWithSubject(s).Select(t=>
                    {
                        string prf;
                        string localName;
                        var ns = GetNsAndLocalName((string) t.Predicate.Content, out localName);
                        if (!prefixes.TryGetValue(ns, out prf))
                        {
                            prefixes.Add(ns, prf = "ns" + i++);
                            RDF.Add(new XAttribute(XNamespace.Xmlns + prf, ns));
                        }
                        var x = new XElement(XName.Get(localName, prf));

                        if (t.Object is IIriNode)
                        {
                            x.Add(new XAttribute(rdf + "resource", t.Object.ToString()));
                        }   
                        else if (t.Object is IBlankNode) //todo
                            {
                                x.Add(new XAttribute(rdf + "nodeID", ((IBlankNode)t.Object).Name));
                            }
                        else
                        {
                            ILiteralNode literal = t.Object as ILiteralNode;
                            if (literal != null)
                            {
                                if (literal is ILanguageLiteral)
                                {
                                    x.Add(new XAttribute(XNamespace.Xml + "lang", ((ILanguageLiteral) t.Object).Lang));
                                }
                                else if (literal is IStringLiteralNode)
                                {
                                }
                                else
                                    x.Add(new XAttribute(rdf + "datatype", literal.DataType));
                                x.Add(literal.Content);
                            }

                            else
                            {
                                throw new ArgumentOutOfRangeException();
                            }
                        }
                        return x;
                    })));
            };
       
            return RDF;
        }

        private static string GetNsAndLocalName(string uri, out string localName)
        {
            var lastIndexOf1 = uri.LastIndexOf('\\');
            var lastIndexOf2 = uri.LastIndexOf('/');
            var lastIndexOf3 = uri.LastIndexOf('#');
            var lastIndex = Math.Max(lastIndexOf1, Math.Max(lastIndexOf2, lastIndexOf3));
            if(lastIndex==-1) throw new Exception();

            localName = uri.Substring(lastIndex+1);

            return uri.Substring(0, lastIndex);
        }

        public static string ToJson(this IGraph g)
        {       
            return string.Format("{{ {0} }}",
                string.Join(","+Environment.NewLine,
                    g.GetAllSubjects().Select(s => string.Format("\"{0}\" : {{ {1} }}",
                        s,
                        string.Join(" , ",
                        g.GetTriplesWithSubject(s).GroupBy(po=> po.Predicate).Select(pGroup =>
                            pGroup.Count() > 1
                                ? string.Format("\"{0}\" : [{1}]", pGroup.Key,
                                    string.Join("," + Environment.NewLine, pGroup.Select(t => t.Object).Select(ToJson)))
                                : string.Format("\"{0}\" : {1}", pGroup.Key, ToJson(pGroup.First().Object))))))));
        }

        public static string ToJson(this ObjectVariants b)
        {
            if (b is IIriNode)
            {
                return $@"{{ ""type"" : ""uri"", ""value"" : ""{b}"" }}";
            }
            else if (b is ILiteralNode)
            {
                var literalNode = ((ILiteralNode) b);
                string content = literalNode.Content.ToString();
                content = content.Replace('"', '\'');
                if (literalNode is ILanguageLiteral)
                {
                    return
                        $@"{{ ""type"" : ""literal"", ""value"" : ""{content}"", ""xml:lang"" : ""{
                            ((ILanguageLiteral) literalNode).Lang}"" }}";
                }
                else if (literalNode is IStringLiteralNode)
                {
                    return $@"{{ ""type"" : ""literal"", ""value"" : ""{content}"" }}";
                }
                else
                {
                    return $@"{{ ""type"" : ""literal"", ""value"" : ""{content}"", ""datatype"" : ""{literalNode.DataType}""}}";
                }
            }
            else if (b is IBlankNode)
            {
                return $@"{{ ""type"" : ""bnode"", ""value"" : ""{ b }"" }}";
            }
            else if (b == null)
                return "";
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static string ToTurtle(this IGraph g)
        {
            return
                string.Join("." + Environment.NewLine,
                    g.GetAllSubjects().Select(s =>
                        string.Format(@"<{0}> 
            {1}", s,
                        string.Join(";" + Environment.NewLine+"          ",
                            g.GetTriplesWithSubject(s)
                            .GroupBy(po => po.Predicate).Select(pGroup =>
                                string.Format("<{0}> {1}", pGroup.Key,
                                    string.Join("," + Environment.NewLine + "                                              ", 
                                    pGroup.Select(t =>
                                    {
                                        if (t.Object is IIriNode)
                                            return "<" + t.Object + ">";
                                        else if ((t.Object is OV_langstring)) return "\"" + t.Object + "\"@" + ((OV_langstring)t.Object).Lang;
                                        else return "\"" + t.Object + "\"^^<" + ((ILiteralNode) t.Object).DataType + ">";
                                    }))))))));
        }

        public static void AddFromXml(this IGraph g, XElement xRDF)
        {
            g.Add(xRDF.Elements().SelectMany(element => Xml2Triples(element,g)));
        }

        private static IEnumerable<TripleStrOV> Xml2Triples(XElement xItem, IGraph graph)
        {
            XNamespace rdf = XNamespace.Get("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            var s = "http://fogid.net/e/" + xItem.Attribute(rdf + "about").Value;
            yield return   new TripleStrOV(s, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", new OV_iri("http://fogid.net/o/"+xItem.Name.ToString()));
            foreach (var xProp in xItem.Elements())
            {
                if (xProp.Name == "iisstore")
                {
                    //var iisstoreNode = graph.NodeGenerator.CreateBlank();
                    //yield return new TripleStrOV(s,"http://fogid.net/o/iisstore",new OV_iri(iisstoreNode));
                    //foreach (var attribute in xProp.Attributes())
                    //    yield return
                    //        new TripleStrOV(iisstoreNode, "http://fogid.net/o/" + attribute.Name,
                    //            attribute.Name == "originalname" || attribute.Name == "uri"
                    //                ? new OV_iri(attribute.Value)
                    //                : (ObjectVariants) String2Node(attribute.Value));
                }
                else
                {
                    var p = "http://fogid.net/o/" + xProp.Name.ToString();
                    var valueIriAttribute = xProp.Attribute(rdf + "resource");
                    //todo lang var valueIriAttribute = xProp.Attribute(rdf+"http://www.w3.org/1999/02/22-rdf-syntax-ns#resource");
                    if (valueIriAttribute != null)
                        yield return new TripleStrOV(s, p, new OV_iri("http://fogid.net/e/" + valueIriAttribute.Value));
                    else if (xProp.Elements().Any())
                        foreach (var xObj in xProp.Elements())
                        {
                            var o = xObj.Attribute(rdf + "about");
                            if (o == null) throw new Exception();
                            yield return new TripleStrOV(s, p, new OV_iri("http://fogid.net/e/" + o.Value));
                            yield return new TripleStrOV(o.Value, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", new OV_iri("http://fogid.net/o/" + xItem.Name.ToString()));
                        }
                    else if (xProp.Attribute(XNamespace.Xml + "lang") != null)
                        yield return new TripleStrOV(s, p, new OV_langstring(xProp.Value, xProp.Attribute(XNamespace.Xml + "lang").Value));
                    else if (xProp.Attribute(rdf + "datatype") != null)
                        yield return new TripleStrOV(s, p, graph.NodeGenerator.CreateLiteralNode(xProp.Value, xProp.Attribute(rdf + "datatype").Value));
                    else
                        yield return new TripleStrOV(s, p, (ObjectVariants)String2Node(xProp.Value));
                }
             
            }
        }

        static ILiteralNode String2Node(string literal)
        {
            //DateTime date;
            //if(DateTime.TryParse(literal, out date))
            //    return new OV_dateTime(date);
            int i;
            if(Int32.TryParse(literal, out i))
                return new OV_int(i);
            long l;
            if (Int64.TryParse(literal, out l))
                return new OV_long(l);
            //todo

            return new OV_string(literal);
        }

        public static XElement ToXml(this IGraph g, Prologue prolog)
        {
            XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
            var prefixes = new Dictionary<string, string>();

            var RDF = new XElement(rdf + "RDF");
            foreach (var s in g.GetAllSubjects())
            {
                XAttribute id = null;
                if (s is IIriNode)
                {
                    id = new XAttribute(rdf + "about", ((ObjectVariants) s).Content);
                }
                else if (s is IBlankNode)
                {
                    id = new XAttribute(rdf + "nodeID", s.ToString());
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
                RDF.Add(new XElement(rdf + "Description", id,
                    g.GetTriplesWithSubject(s)
                    .Select(t=>
                    {
                        string p;
                        NamespaceLocalName ns = Prologue.SplitUri(t.Predicate.Content.ToString());
                        //if (!prefixes.TryGetValue(ns.@namespace, out p))
                        //{ 
                        //    RDF.Add);
                        //}
                        var x = new XElement(XName.Get("ns", ns.localname), new XAttribute(XNamespace.Xmlns + "ns", ns.@namespace));
                        if (t.Object is IIriNode)
                        {
                            x.Add(new XAttribute(rdf + "resource", t.Predicate.Content));
                        }
                        else if (t.Object is ILiteralNode)
                        {
                            ILiteralNode ol = ((ILiteralNode) t.Object);
                            if (ol is ILanguageLiteral)
                                x.Add(new XAttribute(XNamespace.Xml + "lang",
                                    ((ILanguageLiteral) t.Object).Lang));
                            else if (!(ol is IStringLiteralNode))
                                x.Add(new XAttribute(rdf + "datatype", ol.DataType));

                            x.Add(ol.Content);
                        }
                        else if (t.Object is IBlankNode)
                        {
                            x.Add(new XAttribute(rdf + "nodeID", t.Object.ToString()));
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                        return x;
                    })));
            };

            return RDF; 
        }
    }
}