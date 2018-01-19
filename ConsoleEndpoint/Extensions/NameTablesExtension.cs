namespace ConsoleEndpoint
{
    public static class NameTablesExtension
    {
        /// Код, означающий отсутствие кода
        public static int EmptyCode(this INametable nt)
        {
            return int.MinValue;
        }
    }
}