using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlMD5 :SparqlHashExpression
    {
        public SparqlMD5(SparqlExpression value) : base(value)
        {
            this.Create(value);            
        }

        readonly MD5 md5 = new MD5CryptoServiceProvider();
         protected override string CreateHash(string f)
        {
            return string.Join(string.Empty, this.md5.ComputeHash(Encoding.UTF8.GetBytes(f)).Select( b => b.ToString("x2")));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("md5");
            writer.WriteAttributeString("type", this.GetType().ToString());
            base.WriteXml(writer);
        }
    }
}
