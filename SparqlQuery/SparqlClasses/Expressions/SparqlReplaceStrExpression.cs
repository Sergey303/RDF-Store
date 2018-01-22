using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlReplaceStrExpression : SparqlExpression
    {
        private SparqlExpression stringExpression;
        private SparqlExpression parametersExpression;
        private SparqlExpression replacementExpression;
        private SparqlExpression patternExpression;

        internal void SetString(SparqlExpression stringExpression)
        {
            this.stringExpression = stringExpression;
        }

        internal void SetPattern(SparqlExpression patternExpression)
        {
            this.patternExpression = patternExpression;
        }

        internal void SetReplacement(SparqlExpression replacementExpression)
        {
            this.replacementExpression = replacementExpression;

        }

        internal void SetParameters(SparqlExpression parametersExpression)
        {
            this.parametersExpression = parametersExpression;

        }

        public void Create()
        {
            if (this.parametersExpression == null)
            {
                switch (
                    NullableTripleExt.Get(this.stringExpression.Const, this.patternExpression.Const, this.replacementExpression.Const))
                {
                    case NT.AllNull:
                        this.Operator = res =>
                            ((string)this.stringExpression.Operator(res)).Replace(
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Operator(res));
                        this.TypedOperator =
                            res => this.stringExpression.TypedOperator(res)
                                .Change(o => ((string) o).Replace((string)this.patternExpression.Operator(res),
                                    (string)this.replacementExpression.Operator(res)));
                        break;
                    case NT.FirstNotNull:
                        this.Operator = res =>
                            ((string)this.stringExpression.Const.Content).Replace(
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Operator(res));
                        this.TypedOperator =
                            res => this.stringExpression.Const
                                .Change(o => ((string) o).Replace((string)this.patternExpression.Operator(res),
                                    (string)this.replacementExpression.Operator(res)));
                        break;
                    case NT.SecondNotNull:
                        this.Operator = res =>
                            ((string)this.stringExpression.Operator(res)).Replace(
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Operator(res));
                        this.TypedOperator =
                            res => this.stringExpression.TypedOperator(res)
                                .Change(o => ((string) o).Replace((string)this.patternExpression.Const.Content,
                                    (string)this.replacementExpression.Operator(res)));
                        break;
                    case NT.ThirdNotNull:
                        this.Operator = res =>
                            ((string)this.stringExpression.Operator(res)).Replace(
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Const.Content);
                        this.TypedOperator =
                            res => this.stringExpression.TypedOperator(res)
                                .Change(o => ((string) o).Replace((string)this.patternExpression.Operator(res),
                                    (string)this.replacementExpression.Const.Content));
                        break;
                     case ~NT.FirstNotNull:
                        this.Operator = res =>
                            ((string)this.stringExpression.Operator(res)).Replace(
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Const.Content);
                        this.TypedOperator =
                            res => this.stringExpression.TypedOperator(res)
                                .Change(o => ((string) o).Replace((string)this.patternExpression.Const.Content,
                                    (string)this.replacementExpression.Const.Content));
                        break;
                    case ~NT.SecondNotNull:
                        this.Operator = res =>
                            ((string)this.stringExpression.Const.Content).Replace(
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Const.Content);
                        this.TypedOperator =
                            res => this.stringExpression.Const
                                .Change(o => ((string) o).Replace((string)this.patternExpression.Operator(res),
                                    (string)this.replacementExpression.Const.Content));
                        break;
                    case ~NT.ThirdNotNull:
                        this.Operator = res =>
                            ((string)this.stringExpression.Const.Content).Replace(
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Operator(res));
                        this.TypedOperator =
                            res => this.stringExpression.Const
                                .Change(o => ((string) o).Replace((string)this.patternExpression.Const.Content,
                                    (string)this.replacementExpression.Operator(res)));
                        break;
                    case ~NT.AllNull:
                        this.Const = this.stringExpression.Const
                            .Change(o => ((string) o).Replace((string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Const.Content));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (this.parametersExpression.Const != null)
                {
                    var flags = this.GetRegexOptions((string)this.parametersExpression.Const.Content);
                    switch (
                        NullableTripleExt.Get(this.stringExpression.Const, this.patternExpression.Const, this.replacementExpression.Const))
                    {
                        case NT.AllNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Operator(res),
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Operator(res),
                                flags);
                            this.TypedOperator =
                                res => this.stringExpression.TypedOperator(res)
                                    .Change(o => Regex.Replace((string) o, (string)this.patternExpression.Operator(res),
                                        (string)this.replacementExpression.Operator(res),
                                        flags));
                            break;
                        case NT.FirstNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Const.Content,
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Operator(res),
                                flags);
                            this.TypedOperator =
                                res => this.stringExpression.Const
                                    .Change(o => Regex.Replace((string) o, (string)this.patternExpression.Operator(res),
                                        (string)this.replacementExpression.Operator(res),
                                        flags));
                            break;
                        case NT.SecondNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Operator(res),
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Operator(res),
                                flags);
                            this.TypedOperator =
                                res => this.stringExpression.TypedOperator(res)
                                    .Change(o => Regex.Replace((string) o, (string)this.patternExpression.Const.Content,
                                        (string)this.replacementExpression.Operator(res),
                                        flags));
                            break;
                        case NT.ThirdNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Operator(res),
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Const.Content);
                            this.TypedOperator =
                                res => this.stringExpression.TypedOperator(res)
                                    .Change(o => Regex.Replace((string) o, (string)this.patternExpression.Operator(res),
                                        (string)this.replacementExpression.Const.Content,
                                        flags));
                            break;
                        case ~NT.FirstNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Operator(res),
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Const.Content,
                                flags);
                            this.TypedOperator =
                                res => this.stringExpression.TypedOperator(res)
                                    .Change(o => Regex.Replace((string) o, (string)this.patternExpression.Const.Content,
                                        (string)this.replacementExpression.Const.Content,
                                        flags));
                            break;
                        case ~NT.SecondNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Const.Content,
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Const.Content);
                            this.TypedOperator =
                                res => this.stringExpression.Const
                                    .Change(o => Regex.Replace((string) o, (string)this.patternExpression.Operator(res),
                                        (string)this.replacementExpression.Const.Content,
                                        flags));
                            break;
                        case ~NT.ThirdNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Const.Content,
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Operator(res),
                                flags);
                            this.TypedOperator =
                                res => this.stringExpression.Const
                                    .Change(o => Regex.Replace((string) o, (string)this.patternExpression.Const.Content,
                                        (string)this.replacementExpression.Operator(res),
                                        flags));
                            break;
                        case ~NT.AllNull:
                            this.Const = this.stringExpression.Const
                                .Change(o => Regex.Replace((string) o, (string)this.patternExpression.Const.Content,
                                    (string)this.replacementExpression.Const.Content,
                                    flags));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }
                else
                {
                    switch (NullableTripleExt.Get(this.stringExpression.Const, this.patternExpression.Const, this.replacementExpression.Const))
                    {
                        case NT.AllNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Operator(res),
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Operator(res), this.patternExpression.Operator(res));
                            this.TypedOperator =
                                res => this.stringExpression.TypedOperator(res)
                                    .Change(o => Regex.Replace((string)o, (string)this.patternExpression.Operator(res),
                                        (string)this.replacementExpression.Operator(res), this.patternExpression.Operator(res)));
                            break;
                        case NT.FirstNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Const.Content,
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Operator(res), this.patternExpression.Operator(res));
                            this.TypedOperator =
                                res => this.stringExpression.Const
                                    .Change(o => Regex.Replace((string)o, (string)this.patternExpression.Operator(res),
                                        (string)this.replacementExpression.Operator(res), this.patternExpression.Operator(res)));
                            break;
                        case NT.SecondNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Operator(res),
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Operator(res), this.patternExpression.Operator(res));
                            this.TypedOperator =
                                res => this.stringExpression.TypedOperator(res)
                                    .Change(o => Regex.Replace((string)o, (string)this.patternExpression.Const.Content,
                                        (string)this.replacementExpression.Operator(res), this.patternExpression.Operator(res)));
                            break;
                        case NT.ThirdNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Operator(res),
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Const.Content);
                            this.TypedOperator =
                                res => this.stringExpression.TypedOperator(res)
                                    .Change(o => Regex.Replace((string)o, (string)this.patternExpression.Operator(res),
                                        (string)this.replacementExpression.Const.Content, this.patternExpression.Operator(res)));
                            break;
                        case ~NT.FirstNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Operator(res),
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Const.Content, this.patternExpression.Operator(res));
                            this.TypedOperator =
                                res => this.stringExpression.TypedOperator(res)
                                    .Change(o => Regex.Replace((string)o, (string)this.patternExpression.Const.Content,
                                        (string)this.replacementExpression.Const.Content, this.patternExpression.Operator(res)));
                            break;
                        case ~NT.SecondNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Const.Content,
                                (string)this.patternExpression.Operator(res),
                                (string)this.replacementExpression.Const.Content);
                            this.TypedOperator =
                                res => this.stringExpression.Const
                                    .Change(o => Regex.Replace((string)o, (string)this.patternExpression.Operator(res),
                                        (string)this.replacementExpression.Const.Content, this.patternExpression.Operator(res)));
                            break;
                        case ~NT.ThirdNotNull:
                            this.Operator = res => Regex.Replace(
                                (string)this.stringExpression.Const.Content,
                                (string)this.patternExpression.Const.Content,
                                (string)this.replacementExpression.Operator(res), this.patternExpression.Operator(res));
                            this.TypedOperator =
                                res => this.stringExpression.Const
                                    .Change(o => Regex.Replace((string)o, (string)this.patternExpression.Const.Content,
                                        (string)this.replacementExpression.Operator(res), this.patternExpression.Operator(res)));
                            break;
                        case ~NT.AllNull:
                            this.Operator = res => 
                                Regex.Replace((string)this.stringExpression.Const.Content, (string)this.patternExpression.Const.Content,
                                    (string)this.replacementExpression.Const.Content, this.patternExpression.Operator(res));
                            this.TypedOperator =  res=> this.stringExpression.Const
                                .Change(o => Regex.Replace((string)o, (string)this.patternExpression.Const.Content,
                                    (string)this.replacementExpression.Const.Content, this.patternExpression.Operator(res)));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private RegexOptions GetRegexOptions(string flags)
        {
               var  ro = RegexOptions.None;
            if (flags.Contains("s"))
                ro |= RegexOptions.Singleline;
            if (flags.Contains("m"))
                ro |= RegexOptions.Multiline;
            if (flags.Contains("i"))
                ro |= RegexOptions.IgnoreCase;
            if (flags.Contains("x"))
                ro |= RegexOptions.IgnorePatternWhitespace;
            return ro;
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("replace");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.stringExpression.WriteXml(writer);
            this.parametersExpression.WriteXml(writer);
            this.replacementExpression.WriteXml(writer);
            this.patternExpression.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
