using System;

namespace SparqlQuery
{
    /// <summary>
    /// Nullable Pair
    /// </summary>
    public enum NP : byte { bothNull, leftNull, rigthNull, bothNotNull }

    public static class NullablePairExt
    {
        public static NP Get<T1, T2>(T1 left, T2 right) => (NP)((left.Equals(default(T1)) ? 0 : 1) << 1 | (right.Equals(default(T1)) ? 0 : 1));
    }

    /// <summary>
    /// Nullable Triple
    /// </summary>
    [Flags]
    public enum NT : byte { AllNull = 0, FirstNotNull = 1, SecondNotNull = 2, ThirdNotNull = 3 }

    public static class NullableTripleExt
    {
        public static NT Get<T1,T2,T3>(T1 first, T2 second, T3 third)
        {
            return (first.Equals(default(T1)) ? NT.AllNull : NT.FirstNotNull)
                   | (second.Equals(default(T2)) ? NT.AllNull: NT.SecondNotNull)
                   | (third.Equals(default(T3)) ? NT.AllNull: NT.ThirdNotNull);
        }
    }
}
