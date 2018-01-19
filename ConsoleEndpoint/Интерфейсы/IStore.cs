namespace ConsoleEndpoint
{
    using System.Collections.Generic;

    public interface IStore
    {
        void Load(IEnumerable<(string s, string p, OV o)> tripleFlow);

        #region one graph gets

        bool Contains(int s, int p, OV o);

        IEnumerable<OV> spO(int s, int p);

        IEnumerable<int> sPo(int s, OV o);

        IEnumerable<int> Spo(int p, OV o);

        IEnumerable<(int s, OV o)> SpO(int p);

        IEnumerable<(int p, OV o)> sPO(int s);

        IEnumerable<(int s, int p)> SPo(OV o);

        IEnumerable<(int s, int p, OV o)> SPO();

        #endregion
    }
}