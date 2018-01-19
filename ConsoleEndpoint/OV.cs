using System;

namespace ConsoleEndpoint
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    /// Класс объектных вариантов: или new object[] {0, iri} или object[] {1, "stroka"}
    public struct OV : IComparable<OV>, IXmlSerializable
    {
        public OVT OVid { get; set; }

        public object OValue { get; set; }

        public OV(OVT vid, object value)
        {
            this.OVid=vid;
            this.OValue = value;
        }

        public int CompareTo(OV other)
        {
            return Compare(this.OVid, this.OValue, other.OVid, other.OValue);
        }

        public static int Compare(OVT vid1, object ov1, OVT vid2, object ov2)
        {
            var compareVid = vid1.CompareTo(vid2);
            if (compareVid != 0)
            {
                return compareVid;
            }

            switch (vid1)
            {
                case OVT.iri:
                    return ((int)ov1).CompareTo((int)ov2);
                case OVT.@string:
                    return string.Compare((string)ov1, (string)ov2, StringComparison.Ordinal);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
        
         

        public void ReadXml(XmlReader reader)
        {
            OVid = (OVT)reader.ReadContentAsInt();
            switch (OVid)
            {
                case OVT.iri:
                    OValue = reader.ReadContentAsInt();
                    break;
                case OVT.@string:
                    OValue = reader.ReadContentAsString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(this.OVid);
            writer.WriteValue(this.OValue);
        }
        

        public static bool operator !=(OV one, OV other)
        {
            return !(one == other);
        }

        public static bool operator ==(OV one, OV other)
        {
            return one.Equals(other);
        }
    }
}