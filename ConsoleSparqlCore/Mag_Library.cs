using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polar.DB;
using Polar.Cells;
using Polar.CellIndexes;
using RDFCommon.Interfaces;

namespace ConsoleSparqlCore
{
    /// <summary>
    /// Таблица имен. Самый простой вариант: stream1 - последовательность различных строк, stream2 - офсеты начал строк, 
    /// stream3 - множество пар {хеш_строки, офсет_строки}, dic - словарь пар {строка, код}
    /// </summary>
    public class Mag_Nametable : INametable
     {
        private UniversalSequence<int> ids, offs;
        private Dictionary<string, int> dic = new Dictionary<string, int>();
        public Mag_Nametable(Stream stream1, Stream stream2)
        {
            // Если стримы пустые, то создаются последовательности с нулем элементов, если непустые, то происходит подключение
            ids = new UniversalSequence<int>(new PType(PTypeEnumeration.sstring), stream1);
            offs = new UniversalSequence<int>(new PType(PTypeEnumeration.longinteger), stream2);
            int nom = 0;
            ids.Scan((off, ob) =>
            {
                dic.Add((string)ob, nom);
                nom++;
                return true;
            });
        }

        public long LongCount() { return offs.Count(); }

        // получение строки по коду
        public string GetString(int code)
        {
            if (code < 0 || code >= offs.Count()) return null;
            long shift = (long)offs.GetElement(offs.ElementOffset(code));
            return (string)ids.GetElement(shift);
        }

        // Код, означающий отсутствие кода
        public const int Empty = Int32.MinValue;
        // Получение кода по строке
         public void Clear()
         {
             throw new NotImplementedException();
         }

         public int GetCode(string s)
        {
            int nom;
            if (dic.TryGetValue(s, out nom))
            {
                return nom;
            }
            return Empty;
        }
        // Запись кода по строке
        public int GetSetCode(string s)
        {
            int nom;
            if (dic.TryGetValue(s, out nom)) {  }
            else
            {
                nom = (int)ids.Count();
                long off = ids.AppendElement(s);
                offs.AppendElement(off);
                dic.Add(s, nom);
            }
            return nom;
        }

         public Dictionary<string, int> InsertPortion(IEnumerable<string> unsortedNames)
         {
             throw new NotImplementedException();
         }


        public static void TestNameTable(INametable nt)
        {
            System.Diagnostics.Stopwatch T = new System.Diagnostics.Stopwatch();
            Random rnd = new Random();
            string prefix = "PupkinQQUTEUJSHJDHJGFHSGDHWTYTHXCGHGCHSHSDYSTDSHSGHSG_";

            int max = 1_000_000;
            int total = 7 * max;

            T.Restart();
            for (int i=0; i < total; i++)
            {
                string scode = prefix + rnd.Next(max);
                var code = nt.GetSetCode(scode);
            }
            T.Stop();
            Console.WriteLine($" get set {total} codes for new strings time {T.ElapsedMilliseconds}. Nametable Count={nt.LongCount()}");


            int nprobes = 1_000_000;
            T.Restart();
            int unknown = 0;
            for (int j = 0; j < nprobes; j++)
            {
                string scode = prefix + rnd.Next(max);
                int code = nt.GetCode(scode);
                if (code == Int32.MinValue) unknown++;
            }
            T.Stop();
            Console.WriteLine($" get {nprobes} codes. time {T.ElapsedMilliseconds}. {unknown} are unknown");

            //var randomNotExistingStrings = RandomStringsList(getCodeCalls);

            //T.Restart();
            //for (int j = 0; j < getCodeCalls; j++)
            //{
            //    nt.GetCode(randomNotExistingStrings[j]);
            //}
            //T.Stop();
            //Console.WriteLine($" get {getCodeCalls} not existing codes time {T.ElapsedMilliseconds}");


            ////get string test
            ////int getStringCalls = 1000 * 1000;
            //var randomExistingCodes = Enumerable.Range(0, getCodeCalls)
            //    .Select(j => Random.Next(getCodeCalls))
            //    .Select(randomI => allStrings[randomI])
            //    .Select(nt.GetCode)
            //    .ToList();

            //T.Restart();
            //for (int j = 0; j < getCodeCalls; j++)
            //{
            //    nt.GetCode(randomExistingStrings[j]);
            //}
            //T.Stop();
            //Console.WriteLine($" get {getCodeCalls} existing strings time {T.ElapsedMilliseconds}");

        }

    }
}
