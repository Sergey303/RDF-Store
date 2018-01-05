using System.Collections.Generic;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlArgs :List<SparqlExpression>
    {
        public bool isDistinct;

        internal void IsDistinct()
        {
            this.isDistinct=true;
        }

       
    }
}
