using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlSHA512 : SparqlHashExpression
    {
      readonly SHA512 hash=new SHA512CryptoServiceProvider();
            public SparqlSHA512(SparqlExpression value)   :base(value)
        {
                // SetExprType(ObjectVariantEnum.Str);
            // value.SetExprType(ObjectVariantEnum.Str);
            this.Create(value);
        }

      protected override string CreateHash(string f)
        {
            return string.Join(string.Empty, this.hash.ComputeHash(Encoding.UTF8.GetBytes(f)).Select( b => b.ToString("x2")));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("sha512");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
        }
    }
}
