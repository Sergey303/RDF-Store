using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlRegexExpression : SparqlExpression
    {
        private SparqlExpression variableExpression;
        private IStore store;
        private static readonly Dictionary<string, Regex> Regexes = new Dictionary<string, Regex>();
        private static readonly Dictionary<KeyValuePair<string, RegexOptions>, Regex> RegexesParameters = new Dictionary<KeyValuePair<string, RegexOptions>, Regex>();
        private string parameters;
        private RegexOptions ignorePatternWhitespace;
        private static readonly Regex isSimple = new Regex(@"[\w\n\d\s\t_\.\,\!""']*");
        private SparqlExpression pattern;

        internal void SetVariableExpression(SparqlExpression sparqlExpression, IStore store)
        {
            this.variableExpression = sparqlExpression;
            this.store = store;
        }

        internal void SetRegex(SparqlExpression patternExpression)
        {
            var varConst = this.variableExpression.Const;
            this.pattern = patternExpression;
            var patternConst = patternExpression.Const;
            Regex regex;
            switch (NullablePairExt.Get(this.pattern, varConst))
            {
                case NP.bothNull:
                    break;
                case NP.leftNull:
                    this.Operator = result =>
                    {
                        string patternStr = (string) patternExpression.TypedOperator(result).Content;
                            regex = this.CreateRegex(patternStr);        
                        return regex.IsMatch((string) varConst.Content);
                    };
                    this.AggregateLevel = patternExpression.AggregateLevel;
                    break;
                case NP.rigthNull:
                    var content = (string)patternConst.Content;
                    regex = this.CreateRegex(content);
                    var varExpression = this.variableExpression as SparqlVarExpression;
                    if (varExpression != null && isSimple.IsMatch(content))
                    {
                        this.Operator = result =>
                        {
                            if(result.ContainsKey(varExpression.Variable))
                            return regex.IsMatch(varExpression.Operator(result));
                            return this.store.GetTriplesWithTextObject(varExpression.Operator(result))
                                    .Select(this.Selector(result, varExpression.Variable));
                            
                        };
                    }
                    else
                {
                    this.Operator = result => regex.IsMatch(this.variableExpression.Operator(result));
                }

                    this.AggregateLevel = this.variableExpression.AggregateLevel;
                    break;
                case NP.bothNotNull:
                    regex = this.CreateRegex((string)patternConst.Content);
                    this.Const=new OV_bool(regex.IsMatch((string)this.variableExpression.Const.Content));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.TypedOperator = result => new OV_bool(this.Operator(result));
        }

        private Func<TripleIntOV, SparqlResult> Selector(SparqlResult sparqlResult, VariableNode variable)
        {
            return t =>
            {
            sparqlResult[variable] = t.Object;
                return sparqlResult;
            };
        }

        private Regex CreateRegex(string pattern)
        {
            Regex regex;
            
            if (this.parameters == null)
            {

                if (!Regexes.TryGetValue(pattern, out regex))
                {
                    Regexes.Add(pattern, regex = new Regex(pattern));
                }
            }
            else
            {
                var key = new KeyValuePair<string, RegexOptions>(pattern, this.ignorePatternWhitespace);
                if (!RegexesParameters.TryGetValue(key, out regex))
                    RegexesParameters.Add(key, regex = new Regex(pattern, this.ignorePatternWhitespace)); 
            }

            return regex;
        }


        internal void SetParameters(SparqlExpression paramsExpression)
        {
            this.parameters = (string) paramsExpression.Const.Content;
            this.ignorePatternWhitespace = RegexOptions.None;
            if (this.parameters.Contains("s")) this.ignorePatternWhitespace |= RegexOptions.Singleline;
            if (this.parameters.Contains("m")) this.ignorePatternWhitespace |= RegexOptions.Multiline;
            if (this.parameters.Contains("i")) this.ignorePatternWhitespace |= RegexOptions.IgnoreCase;
            if (this.parameters.Contains("x")) this.ignorePatternWhitespace |= RegexOptions.IgnorePatternWhitespace;
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("regex");
            writer.WriteAttributeString("type", this.GetType().ToString());
            writer.WriteAttributeString("parameters", this.parameters);
            this.variableExpression.WriteXml(writer);
            this.pattern.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
