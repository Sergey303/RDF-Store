using System;
using System.IO;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;

namespace RDFStore
{
    public class Store : RDFGraph, IStore //CacheMeasure  GraphCached   InterpretMeasure
    {

        public Store(string path)
            //        : base(new SecondStringGraph(path)) 
            : base(path)
        {
            NamedGraphs = new NamedGraphsByFolders(new DirectoryInfo(path), ng, d => new RDFGraph(d.FullName + "/") { NodeGenerator = NodeGenerator },
                d => { d.Delete(true); });
        }

        private readonly NodeGeneratorInt ng;

        public void ReloadFrom(long iri_Count, string fileName)
        {
           FromTurtle(iri_Count, fileName);
           // ActivateCache();
        }
        
      


        public void ReloadFrom(string filePath)
        {
            this.ClearAll();
            FromTurtle(1000*1000, filePath);

        }

        public IStoreNamedGraphs NamedGraphs { get; set; }
        public void ClearAll()
        {
            base.Clear();
        }

        public IGraph CreateTempGraph()
        {
            throw new NotImplementedException();
            // return new RamListOfTriplesGraph("temp");
        }

     
    }
}
