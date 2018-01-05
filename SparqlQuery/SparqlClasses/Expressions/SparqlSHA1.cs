using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace SparqlQuery.SparqlClasses.Expressions
{

    class SparqlSHA1 :  SparqlHashExpression
    {
        private readonly SHA1 hash;
             public SparqlSHA1(SparqlExpression value)    :base(value)
        {
            this.Create(value);    
        }

        protected override string CreateHash(string f)
        {
            return string.Join(string.Empty, this.hash.ComputeHash(Encoding.UTF8.GetBytes(f)).Select( b => b.ToString("x2")));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("sha1");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
        }
    }
}
