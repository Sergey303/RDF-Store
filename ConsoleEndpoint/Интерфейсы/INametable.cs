namespace ConsoleEndpoint
{
    public interface INametable
    {
        int GetCode(string s);

        string GetString(int c);

        int GetSetCode(string s);

        void Clear();

        long LongCount();
    }
}