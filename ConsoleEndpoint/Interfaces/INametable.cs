namespace ConsoleEndpoint.Interfaces
{
    public interface INametable
    {
        int GetCode(string s, out bool exists);

        string GetString(int c);

        int GetSetCode(string s);

        void Clear();

        long LongCount();
    }
}