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
            // todo array of array of OVT 
        }

        public bool Contains(int s, int p, object[] o)
        {
            RequeresObjectVariant(o);
            return false;
        }

        public IEnumerable<object[]> spO(int s, int p)
        {
            return null;
        }

        public IEnumerable<object[]> sPo(int s, object[] o)
        {
            RequeresObjectVariant(o);
            return null;
        }

        public IEnumerable<object[]> Spo(int p, object[] o)
        {
            RequeresObjectVariant(o);
            return null;
        }

        public IEnumerable<object[]> SpO(int p)
        {
            return null;
        }

        public IEnumerable<object[]> sPO(int s)
        {
            return null;
        }
      
        private static void RequeresObjectVariant(object[] ov)
        {
            Contract.Requires(ov != null);
            Contract.Requires(ov.Length == 2);
            Contract.Requires(ov[0] is int);
        }

        public IEnumerable<object[]> SPO()
        {

            return null;
        }
    }
}