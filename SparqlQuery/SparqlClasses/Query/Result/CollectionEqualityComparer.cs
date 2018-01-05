using System.Collections.Generic;
using RDFCommon.OVns;

namespace SparqlQuery.SparqlClasses.Query.Result
{
    public class CollectionEqualityComparer : IEqualityComparer<List<ObjectVariants>>
    {
        public bool Equals(List<ObjectVariants> x, List<ObjectVariants> y)
        {
            if(x.Count!=y.Count) return false;
            for (int i = 0; i < x.Count; i++)
                if (! x[i].Equals(y[i])) return false;
            return true;
        }

        public int GetHashCode(List<ObjectVariants> obj)
        {
            unchecked
            {
                int sum=0;
                for (int i = 0; i < obj.Count; i++)
                {
                    sum += obj[i].GetHashCode();
                }

                return sum;
            }
        }

     
    }
}