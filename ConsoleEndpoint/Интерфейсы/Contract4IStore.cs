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
        
        public INametable Nametable { get; }

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(Nametable!=null);
        }

        public bool ContainsObject(object[] o)
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(int s, int p, object[] o)
        {
            RequeresObjectVariant(o);
            return false;
        }

        public IEnumerable<object[]> spO(int s, int p)
        {
            Contract.Ensures(Contract.Result<object[]>() != null);
            return null;
        }

        public IEnumerable<object[]> sPo(int s, object[] o)
        {
            RequeresObjectVariant(o);
            Contract.Ensures(Contract.Result<object[]>() != null);
            return null;
        }

        public IEnumerable<object[]> Spo(int p, object[] o)
        {
            RequeresObjectVariant(o);
            Contract.Ensures(Contract.Result<object[]>() != null);
            return null;
        }

        public IEnumerable<object[]> SpO(int p)
        {
            Contract.Ensures(Contract.Result<object[]>() != null);
            return null;
        }

        public IEnumerable<object[]> sPO(int s)
        {
            Contract.Ensures(Contract.Result<object[]>() != null);
            return null;
        }

        public IEnumerable<object[]> SPo(object[] o)
        {
            Contract.Ensures(Contract.Result<object[]>() != null);
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

            Contract.Ensures(Contract.Result<object[]>() != null);
            return null;
        }
    }
}