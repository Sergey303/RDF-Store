using RDFCommon;
using RDFCommon.Interfaces;

namespace RDFStore
{
   public class RDFRamStore :RamListOfTriplesGraph, IStore
    {

        public IStoreNamedGraphs NamedGraphs { get; }
        public void ClearAll()
        {
            Clear();
        }

        public IGraph CreateTempGraph()
        {
            return new RamListOfTriplesGraph("temp");
        }

        public void ReloadFrom(string filePath)
        {
           this.ClearAll();
            base.FromTurtle(10, filePath);
        }
        
    }
}
