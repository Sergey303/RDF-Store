namespace ConsoleEndpoint.Interfaces
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(Contract4IStore))]
    public interface IStore
    {
        void Load(IEnumerable<object> tripleFlow);

        INametable Nametable { get; }

        /// <summary>
        /// Проверка наличия объекта - константы в данных, равносильная get code
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool ContainsObject(object[] o);

        #region one graph gets

        bool Contains(int s, int p, object[] o);

        IEnumerable<object[]> spO(int s, int p);

        IEnumerable<object[]> sPo(int s, object[] o);

        IEnumerable<object[]> Spo(int p, object[] o);

        IEnumerable<object[]> SpO(int p);

        IEnumerable<object[]> sPO(int s);

        IEnumerable<object[]> SPo(object[] o);

        IEnumerable<object[]> SPO();

        #endregion
    }
}