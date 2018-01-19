using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Update;

namespace SparqlQuery.SparqlClasses.Query.Result
{
    [Serializable]
    public class SparqlResultSet 
    {
        private readonly RdfQuery11Translator q;
        public IEnumerable<SparqlResult> Results ;
        public IGraph GraphResult;
        public ResultType ResultType;
        public SparqlQuery Query;
      

        public bool AnyResult
        {
            get { return this.Results.Any(); }
        }

        public string UpdateMessage;

        public SparqlUpdateStatus UpdateStatus;

        internal Dictionary<string, VariableNode> Variables = new Dictionary<string, VariableNode>();
        private readonly Prologue prologue;

        public SparqlResultSet(SparqlQuery query, Prologue prologue=null)
        {
            this.Query = query;
            this.prologue = prologue;
        }

        public XElement ToXml()
        {
            XNamespace xn = "http://www.w3.org/2005/sparql-results#";
            switch (this.ResultType)
            {
                case ResultType.Select:
                    return new XElement(xn + "sparql", new XAttribute(XNamespace.Xmlns + "ns", xn),
                        new XElement(xn + "head", this.Variables.Select(v => new XElement(xn + "variable", new XAttribute(xn + "name", v.Key)))),
                        new XElement(xn + "results", this.Results.Select(result =>
                                new XElement(xn + "result",
                                    ToXML(result)))));
                case ResultType.Describe:
                case ResultType.Construct:
                    return this.GraphResult.ToXml(this.prologue);
                case ResultType.Ask:
                    return new XElement(xn + "sparql", // new XAttribute(XNamespace.Xmlns , xn),
                        new XElement(xn + "head", this.Variables.Select(v => new XElement(xn + "variable", new XAttribute(xn + "name", v.Key)))),
                        new XElement(xn + "boolean", this.AnyResult));
                case ResultType.Update:
                    return new XElement("update", new XAttribute("status", this.UpdateStatus.ToString()));
                default:                                       
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IEnumerable<XElement> ToXML(SparqlResult result)
        {
            XNamespace xn = "http://www.w3.org/2005/sparql-results#";
            return result.GetSelected((var, value)=> 
                new XElement(xn + "binding",    
                    new XAttribute(xn + "name", var.VariableName),
                    BindingToXml(xn, value)));
        }

        // public JsonConvert ToJson()
        public static XElement BindingToXml(XNamespace xn, ObjectVariants b)
        {
            if (b is IIriNode)
            {
                return new XElement(xn + "uri", ((IIriNode)b).UriString);
            }     
            else if (b is IBlankNode blank)
            {
                return new XElement(xn + "bnode", blank.Name);
            }
            else if (b is ILiteralNode literalNode)
            {
                if (literalNode is ILanguageLiteral langed)
                {
                    return new XElement(xn + "literal",
                        new XAttribute(xn + "lang", (langed.Lang)), literalNode.Content);
                }
                else if (literalNode is IStringLiteralNode)
                {
                    return new XElement(xn + "literal", literalNode.Content);
                }
                else
                {
                    // if (literalNode == LiteralType.TypedObject)
                    return new XElement(xn + "literal", new XAttribute(xn + "type", literalNode.DataType),
                        literalNode.Content);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            switch (this.ResultType)
            {
                case ResultType.Describe:
                case ResultType.Construct:
                    return this.GraphResult.ToString();
                case ResultType.Select:
                    throw new NotImplementedException(); // return Results.ToString();
                case ResultType.Ask:
                    return this.AnyResult.ToString();

                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public string ToJson()
        {
            string headVars;
            switch (this.ResultType)
            {
                case ResultType.Select:
                        headVars = string.Format(@"""head"": {{ ""vars"": [ {0} ] }}",
                            string.Join("," + Environment.NewLine, this.Variables.Keys.Select(v => string.Format("\"{0}\"", v))));
                    return
                     string.Format(@"{{ {0}, ""results"": {{ ""bindings"" : [{1}] }} }}", headVars, this.SelectToJson() );
                case ResultType.Describe:
                case ResultType.Construct:
                    return this.GraphResult.ToJson();
                case ResultType.Ask:
                    headVars = string.Format(@"""head"": {{ ""vars"": [ {0} ] }}",
                        string.Join("," + Environment.NewLine, this.Variables.Keys.Select(v => string.Format("\"{0}\"", v))));
                    return string.Format("{{ {0}, \"boolean\" : {1}}}", headVars, this.AnyResult);
                case ResultType.Update:
                    headVars = string.Format(@"""head"": {{ ""vars"": [ {0} ] }}",
                        string.Join("," + Environment.NewLine, this.Variables.Keys.Select(v => string.Format("\"{0}\"", v))));
                    return string.Format("{{ {0}, \"status\" : \"{1}\"}}", headVars, this.UpdateStatus);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string SelectToJson()
        {
            return string.Join("," + Environment.NewLine, this.Results.Select(ToJson));
        }

        public static string ToJson(SparqlResult result)
        {
            return string.Format("{{{0}}}",
                string.Join("," + Environment.NewLine,
                    result.GetSelected((var, value) =>
                        string.Format("\"{0}\" : {1}", var.VariableName.Replace("?", string.Empty),
                            value.ToJson()))));
        }

        public static string GetValue(VariableNode vari, ObjectVariants value)
        {
            return value.ToString();
        }

        public string ToCommaSeparatedValues()
        {
            string headVars;
            switch (this.ResultType)
            {
                case ResultType.Select:
                    headVars = string.Join(",", this.Variables.Keys);
                    return
                      headVars+Environment.NewLine+
                         string.Join(Environment.NewLine, this.Results.Select(result=>
                             string.Join(",",
                             result.GetSelected((var, value) => value))));
                case ResultType.Describe:
                case ResultType.Construct:
                    return this.GraphResult.ToTurtle();
                case ResultType.Ask:
                    return this.AnyResult.ToString();
                case ResultType.Update:
                    return this.UpdateStatus.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string FromCommaSeparatedValues()
        {
         
            throw new NotImplementedException();
        }

        public static IEnumerable<SparqlResult> FromXml(XElement load, RdfQuery11Translator q)
        {
            XNamespace xn = "http://www.w3.org/2005/sparql-results#";

            return load
                .Element(xn + "results")
                .Elements()
                .Select(xResult => new SparqlResult(q).Add(xResult.Elements()
                    .Select(xb =>
                    {
                        var variable = q.GetVariable(xb.Attribute(xn + "name").Value);
                        var node = xb.Elements().FirstOrDefault();
                        return new KeyValuePair<VariableNode, ObjectVariants>(variable, Xml2Node(xn, node, q));
                    })));
        }

        private static ObjectVariants Xml2Node(XNamespace xn, XElement b, RdfQuery11Translator q)
        {
            if (b.Name == xn + "uri")
            {
                return new OV_iri(q.prolog.GetFromString(b.Value));
            }
            else if (b.Name == xn + "bnode")
            {
                return q.CreateBlankNode(b.Value);
            }
            else if (b.Name == xn + "literal")
            {
                var lang = b.Attribute(xn + "lang");
                var type = b.Attribute(xn + "type");
                if (lang != null)
                    return new OV_langstring(b.Value, lang.Value);
                else if (type != null)
                    return q.Store.NodeGenerator.CreateLiteralNode(b.Value, q.prolog.GetFromString(type.Value));
                else return new OV_string(b.Value);
            }

            throw new ArgumentOutOfRangeException();
        }

        public bool Equals(SparqlResultSet o)
        {
            if (o.Variables.Count != this.Variables.Count) return false;
           return new HashSet<SparqlResult>(this.Results).SetEquals(o.Results);
        }
    
    }


    public enum ResultType
    {
        Describe, Select, Construct, Ask,
        Update
    }

   
}