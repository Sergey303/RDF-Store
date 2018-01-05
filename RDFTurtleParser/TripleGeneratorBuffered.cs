using System;
using System.Collections.Generic;
using System.IO;
using RDFCommon;
using RDFCommon.Interfaces;

namespace RDFTurtleParser
{
    public class TripleGeneratorBuffered : IGenerator<List<TripleStrOV>>
    {
        private TriplesGenerator tg;
        private List<TripleStrOV> buffer;
        private int maxBuffer;

        public TripleGeneratorBuffered(string path, string graphName, int maxBuffer = 1000000)
        {
            this.maxBuffer = maxBuffer;
            buffer = new List<TripleStrOV>();
            tg = new TriplesGenerator(path);
        }

        public TripleGeneratorBuffered(Stream baseStream, string graphName, int maxBuffer = 1000000)
        {
            this.maxBuffer = maxBuffer;
            buffer = new List<TripleStrOV>();
            tg = new TriplesGenerator(baseStream);
        }

        public void Start(Action<List<TripleStrOV>> onGenerate)
        {
            
            tg.Start( 
                triple =>
                {
                    buffer.Add(triple);
                    if (buffer.Count == maxBuffer)
                    {
                        onGenerate(buffer);
                        buffer=new List<TripleStrOV>();
                    }
                });
            onGenerate(buffer);
        }

  
    }
}