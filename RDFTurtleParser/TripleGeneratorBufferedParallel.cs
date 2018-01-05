using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RDFCommon;
using RDFCommon.Interfaces;

namespace RDFTurtleParser
{
    /// <summary>
    /// читает RDF Turtle с помощью coco/r
    /// запускает <see cref="TripleGeneratorBuffered"/> в отдельной нити,
    /// непрерывно читает входной файл и записывает получаемые порции в Queue<List<Triple<string, string, ObjectVariants>>>
    /// в основном исполняемом потоке отслеживается Queue: если есть элементы, то они "возвращаются" механизмом выполнения указанного делегата.
    /// </summary>
    public class TripleGeneratorBufferedParallel : IGenerator<List<TripleStrOV>>
    {
        private readonly int maxQueue;
        private TripleGeneratorBuffered tg;

        public TripleGeneratorBufferedParallel(string path, string graphName, int maxBuffer = 1000000, int maxQueue = 1000*1000)
        {
            this.maxQueue = maxQueue;
            tg = new TripleGeneratorBuffered(path, graphName, maxBuffer);
        }

        public TripleGeneratorBufferedParallel(Stream baseStream, string graphName, int maxBuffer = 1000000, int maxQueue = 1000*1000)
        {
            this.maxQueue = maxQueue;
             tg = new TripleGeneratorBuffered(baseStream, graphName, maxBuffer);

        }

        /// <summary>
        ///  запускает чтение с помощью TripleGeneratorBuffered в отдельном потоке.
        /// синхронизация буферов с помощью очереди.  
       /// </summary>
        /// <param name="onGenerate"> в основном потоке вынимает из очереди порции и выполняет onGenerate</param>
        public void Start(Action<List<TripleStrOV>> onGenerate)
        {
            var queue = new Queue<List<TripleStrOV>>();

            var thread = new Thread(() =>
                tg.Start(b =>
                {
                    int count;
                    lock (queue)
                        count = queue.Count;
                    while (count == maxQueue)
                    {
                        Thread.Sleep(10);
                        lock (queue)
                            count = queue.Count;
                    }
                    lock (queue)
                        queue.Enqueue(b);
                }));
            thread.Start();
            while (true)
            {
                int count;
                lock (queue)
                    count = queue.Count;
                if (count == 0)
                {
                    if (!thread.IsAlive) break;
                    Thread.Sleep(1);
                }
                else
                {
                    List<TripleStrOV> buffer;
                    lock (queue)
                        buffer = queue.Dequeue();
                    onGenerate(buffer);
                }

            }
        }
    }
}