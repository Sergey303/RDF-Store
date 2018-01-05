using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace SparqlQuery.SparqlClasses.Expressions
{
    internal class SparqlSHA384 : SparqlHashExpression
    {
        private readonly SHA384 hash = new SHA384CryptoServiceProvider();

        public SparqlSHA384(SparqlExpression value)   :base(value)
        {
            // SetExprType(ObjectVariantEnum.Str);
            // value.SetExprType(ObjectVariantEnum.Str);
            this.Create(value);
        }

        protected override string CreateHash(string f)
        {
            return string.Join(string.Empty, this.hash.ComputeHash(Encoding.UTF8.GetBytes(f)).Select(b => b.ToString("x2")));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("sha384");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
        }
    }
}
