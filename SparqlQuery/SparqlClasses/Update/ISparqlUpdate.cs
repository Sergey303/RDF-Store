using RDFCommon;
using RDFCommon.Interfaces;

namespace SparqlQuery.SparqlClasses.Update
{
    using System.Xml.Serialization;

    public interface ISparqlUpdate : IXmlSerializable
    {
        void Run(IStore store);
    }
}