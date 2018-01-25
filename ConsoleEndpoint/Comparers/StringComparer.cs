namespace ConsoleEndpoint.Comparers
{
    using System.Collections.Generic;

    public class StringComparer : IComparer<string>
    {

        #region Singleton
        private readonly static object c_syncLockObject = new object();
        private volatile static StringComparer c_singleton;

        private StringComparer()
        {
        }

        public static StringComparer Default
        {
            get
            {
               // todo if (null == c_singleton)
                {
                    lock (c_syncLockObject)
                    {
                        if (null == c_singleton)
                        {
                            c_singleton = new StringComparer();
                        }
                    }
                }
                return c_singleton;
            }
        }
        #endregion

        public int Compare(string x, string y)
        {
            int comparison = x.Length.CompareTo(y.Length);
            if (comparison != 0) return comparison;
            var xCharArray = x.ToCharArray();
            var yCharArray = y.ToCharArray();
            for (int i = 0; i < xCharArray.Length; i++)
            {
                comparison = xCharArray[i].CompareTo(yCharArray[i]);
                if (comparison != 0) return comparison;
            }
            return 0;
        }
    }
}