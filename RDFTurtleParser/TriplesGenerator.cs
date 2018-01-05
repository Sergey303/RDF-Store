using System;
using System.IO;
using RDFCommon;
using RDFCommon.Interfaces;

namespace RDFTurtleParser
{
    public class TriplesGenerator : IGenerator<TripleStrOV>
    {
        private readonly Parser parser;

        public TriplesGenerator(string path)
        {
            parser = new Parser(new Scanner(path));
            
        }

        public TriplesGenerator(Stream baseStream)
        {
            parser = new Parser(new Scanner(baseStream));
        }


        public void Start(Action<TripleStrOV> onGenerate)
        {
            parser.ft = (s, s1, arg3) => onGenerate(new TripleStrOV(s, s1, arg3));
            parser.Parse();
        }
    }
}
