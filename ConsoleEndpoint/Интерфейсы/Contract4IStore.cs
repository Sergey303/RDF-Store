namespace ConsoleEndpoint.Interface
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    [ContractClassFor(typeof(IStore))]
    public abstract class Contract4IStore : IStore
    {
        private static readonly int IriVid = (int)OVT.iri;

        public void Load(IEnumerable<object> tripleFlow)
        {
            Contract.Requires(tripleFlow.GetType().GetGenericArguments()[0].IsArray);
            // todo array of array of OVT and ints ...
        }

        public bool Contains(object[] s, object[] p, object[] o)
        {
            RequeresIri(s);
            RequeresIri(p);
            RequeresObjectVariant(o);
            return default(bool);
        }

        private static void RequeresObjectVariant(object[] ov)
        {
            Contract.Requires(ov != null);
            Contract.Requires(ov.Length == 2);
            Contract.Requires(ov[0] is int);
        }

        private static void RequeresIri(object[] iri)
        {
            Contract.Requires(iri != null);
            Contract.Requires(iri.Length == 2);
            Contract.Requires(iri[0] is int i);
            Contract.Requires(i == IriVid);
        }

        public IEnumerable<object[]> spO(object[] s, object[] p)
        {
            RequeresIri(s);
            RequeresIri(p);
            return null;
        }

        public IEnumerable<object[]> sPo(object[] s, object[] o)
        {
            RequeresIri(s);
            RequeresObjectVariant(o);
            return null;
        }

        public IEnumerable<object[]> Spo(object[] p, object[] o)
        {

            RequeresIri(p);
            RequeresObjectVariant(o);
            return null;
        }

        public IEnumerable<object[]> SpO(object[] p)
        {

            RequeresIri(p);
            return null;
        }

        public IEnumerable<object[]> sPO(object[] s)
        {
            RequeresIri(s);
            return null;
        }

        public IEnumerable<object[]> SPo(object[] o)
        {

            RequeresObjectVariant(o);
            return null;
        }

        public IEnumerable<object[]> SPO()
        {

            return null;
        }
    }
}