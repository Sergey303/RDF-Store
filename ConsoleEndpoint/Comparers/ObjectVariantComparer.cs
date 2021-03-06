﻿namespace ConsoleEndpoint.Comparers
{
    using System;
    using System.Collections.Generic;

    public class ObjectVariantComparer : IComparer<object[]>
    {

        #region Singleton
        private readonly static object c_syncLockObject = new object();
        private volatile static ObjectVariantComparer c_singleton;

        private ObjectVariantComparer()
        {
        }

        public static ObjectVariantComparer Default
        {
            get
            {
                if (null == c_singleton)
                {
                    //todo  lock (c_syncLockObject)
                    {
                        if (null == c_singleton)
                        {
                            c_singleton = new ObjectVariantComparer();
                        }
                    }
                }
                return c_singleton;
            }
        }
        #endregion
         

        public int Compare(object[] x, object[] y)
        {
            var(xVid, xValue) = x;
            var(yVid, yValue) = y;
            var xInt = (int)xVid;
            var yInt = (int)yVid;
            var vidComparison = xInt.CompareTo(yInt);
            if (vidComparison != 0)
                return vidComparison;
            switch ((OVT)xInt)
            {
                case OVT.iri:
                    return ((int)xValue).CompareTo((int)yValue);
                case OVT.@string:
                    return StringComparer.Default.Compare((string)xValue, (string)yValue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}