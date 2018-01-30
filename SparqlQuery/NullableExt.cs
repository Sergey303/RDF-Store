using System;

namespace SparqlQuery
{
    /// <summary>
    /// Nullable Pair
    /// </summary>
    public enum NP : byte { bothNull, leftNull, rigthNull, bothNotNull }

    public static class NullablePairExt
    {
        public static NP Get(object left, object right)
        {
            return (NP)(((left != null ? 1 : 0) << 1) | (right != null ? 1 : 0));
        }
    }

    /// <summary>
    /// Nullable Triple
    /// </summary>
    [Flags]
    public enum NT : byte { AllNull = 0, FirstNotNull = 1, SecondNotNull = 2, ThirdNotNull = 4 }

    public static class NullableTripleExt
    {
        public static NT Get(object first, object second, object third)
        {
            return (first != null ? NT.FirstNotNull : NT.AllNull)
                   | (second != null ? NT.SecondNotNull : NT.AllNull)
                   | (third != null ? NT.ThirdNotNull : NT.AllNull);
        }
    }
}
