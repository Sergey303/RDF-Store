using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RDFCommon.long_dictionary
{
   public static class MultiMerge
    {
        public static void MainTest(string[] args)
        {
            long[] data=new long[2000*1000 * 1000];

            int partsCount = 10;
            long partSize = data.Length/partsCount+1;
            Random r = new Random();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = r.Next();
            }
            Stopwatch timer=new Stopwatch();
            timer.Start();
            for (int i = 0; i < partsCount-1; i++)
                Array.Sort(data, i * (int)partSize, (int)partSize);
            int lastBufferSize = data.Length - (partsCount - 1)*(int) partSize;
                Array.Sort(data, (partsCount - 1) * (int)partSize, lastBufferSize);
            timer.Stop();
           
            timer.Restart();
     //       var dataSorted =  MergePartialSorted(partsCount, partSize, i => data[i], lastBufferSize, e=>e,);
            timer.Stop();
         
        }


       /// <summary>
       /// Last step to sort one long array (stream) of data is Merging all parts at one methods call.
       /// Parts must be already sorted./// 
       /// </summary>
       /// <typeparam name="TKey">Type to compare elements by</typeparam>
       /// <typeparam name="TValue">Type of elements</typeparam>
       /// <param name="partsCount">nuber of parts</param>
       /// <param name="getNextElement">Method for getting elemtnt from part.</param>
       /// <param name="keyfunction">Method to compute key from element to compare element with other</param>
       /// <param name="comparer">Comparer of keys of elements. If null then used <code>IComparer TKey.default</code> </param>
       /// <param name="partSize"></param>
       /// <param name="elementCastOrCreate"></param>
       /// <param name="isPartEmpty">Method to test are there elements in part</param>
       /// <returns></returns>
       public static IEnumerable<TValue> MergePartialSorted<TKey, TValue>(uint partsCount, Func<int, ulong, TValue> getNextElement, Func<TValue, TKey> keyfunction, IComparer<TKey> comparer, Func<int, ulong> partSize)
       {
           if (comparer == null)
               comparer = Comparer<TKey>.Default;
           //firstElements  - sorted linked list of first elements from each buffer.
            var firstElementsList = new List<MergingKeyValue<TKey, TValue>>();
            var lengths=new List<ulong>((int) partsCount);
            for (int i = 0; i < partsCount; i++)
            {
                var partLength = partSize(i);
                if (partLength <= 0) continue;
                var nextElement = getNextElement(i, 0);
                firstElementsList.Add(new MergingKeyValue<TKey, TValue>(keyfunction(nextElement), nextElement, i, 1));
                lengths.Add(partLength);
            }
            var firstElements = new LinkedList<MergingKeyValue<TKey, TValue>>(firstElementsList.OrderBy(value => value.Key, comparer));

           
           while (firstElements.Count>0)
           {
                var firstNode = firstElements.First;
                var firstLowestElement = firstNode.Value;
               yield return firstLowestElement.Value;
               firstElements.RemoveFirst();
                int indexLowest = firstLowestElement.SortedPartIndex;
                ulong positionInPart = firstLowestElement.SortedPartPosition;
                if (lengths[indexLowest] <= positionInPart)
               {
                   partsCount--;
                //   Console.WriteLine("one part empty");
               }
               else
               {
                   if (firstElements.Count == 0)
                   {
                       if (partsCount != 1)
                           throw new Exception("sdfsdf");
                        //   firstElements.AddFirst(new MergingKeyValue<TKey, TValue>(nextElement.Key, nextElement.Value, buffer));
                       
                       //one last buffer, just return its elements.
                   //    Console.WriteLine("last buffer");
                       for (;
                           positionInPart < lengths[indexLowest];
                           positionInPart++)
                           yield return
                               getNextElement(indexLowest, positionInPart);
                   }
                   else
                   {   // take next element from buffer
                        var nextElement = getNextElement(indexLowest, positionInPart);
                       TKey nextElementKey=keyfunction(nextElement);
                       firstLowestElement.Key = nextElementKey;
                       firstLowestElement.SortedPartPosition = positionInPart + 1;
                       firstLowestElement.Value = nextElement;
                       firstNode.Value = firstLowestElement;

                        // insert next element in firstElements
                        var node = firstElements.First;
                        while (comparer.Compare(nextElementKey, node.Value.Key) > 0)
                       {
                           node = node.Next;
                           if (node != null) continue;
                           // inserting element is biggest, insert it as last.
                           firstElements.AddLast(firstNode);
                           break;
                       }
                       if (node != null) // finded inserted elements position -  bigger element.
                           firstElements.AddBefore(node, firstNode);
                   }
               }
           }
       }

       struct MergingKeyValue<TKey, TValue>
       {
           public TKey Key;
           public TValue Value;
           public readonly int SortedPartIndex;
            public ulong SortedPartPosition;

           public MergingKeyValue(TKey key, TValue value, int sortedPartIndex, ulong sortedPartPosition)
            {
               SortedPartIndex = sortedPartIndex;
               SortedPartPosition = sortedPartPosition;
               Value = value;
               Key = key;
           }
       }


   

  

    }
}
