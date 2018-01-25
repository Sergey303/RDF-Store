namespace ConsoleEndpoint.Interface
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(Contract4IStore))]
    public interface IStore
    {
        void Load(IEnumerable<object> tripleFlow);

        #region one graph gets

        bool Contains(object[] s, object[] p, object[] o);

        IEnumerable<object[]> spO(object[] s, object[] p);

        IEnumerable<object[]> sPo(object[] s, object[] o);

        IEnumerable<object[]> Spo(object[] p, object[] o);

        IEnumerable<object[]> SpO(object[] p);

        IEnumerable<object[]> sPO(object[] s);

        IEnumerable<object[]> SPo(object[] o);

        IEnumerable<object[]> SPO();

        #endregion
    }
}