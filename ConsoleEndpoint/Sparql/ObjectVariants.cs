using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using RDFCommon.Interfaces;
using RDFCommon.OVns.general;

namespace RDFCommon.OVns
{
    public abstract class ObjectVariants   : IComparable, IXmlSerializable
    {
        public  abstract ObjectVariantEnum Variant { get; }
        public abstract object WritableValue { get; }

        public virtual object[] ToWritable()
        {
            return new object[]{(int)Variant, WritableValue};
        }
        

        public virtual int CompareTo(object obj)
        {
            if (obj is ObjectVariants)
            {
                var other = (ObjectVariants)obj;
                return Variant.CompareTo(other.Variant);  
            }
            throw new ArgumentException();
        }

        //public override string ToString()
        //{
        //    return "\""+Content+"\"^^<"+((ILiteralNode)this).DataType+">";
        //}

        public override int GetHashCode()
        {
            //return (int) (Content.ToString() + Variant).GetHashModifiedBernstein();
            return (int)(Content.ToString() + Variant).GetHashCode();
        }

        public abstract object Content { get; }
        public abstract ObjectVariants Change(Func<dynamic, dynamic> changing);

        public static string GenerateBlankIri(string graphName)
        {
            return "generated_blank_iri_of_" + graphName + "_" + Guid.NewGuid().ToString();
        }
        public static string GenerateBlankIri(string graphName, string blank_name)
        {
            return "generated_blank_iri_of_" + graphName + "_" + blank_name;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(this.Variant.ToString());
            if (Variant == ObjectVariantEnum.LanguagedString)
            {
                //writer.WriteAttributeString("lang", ((OV_langstring)this).Lang);
            }
            else if (Variant == ObjectVariantEnum.Typed)
            {
                //writer.WriteAttributeString("type", ((OV_typed)this).DataType);
            }
            
            
                writer.WriteString(Content.ToString());
            
            writer.WriteEndElement();
        }
    }
}