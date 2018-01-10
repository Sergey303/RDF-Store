using System;
using System.Collections.Generic;
using System.IO;
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
            }
            return nom;
        }

         public Dictionary<string, int> InsertPortion(IEnumerable<string> unsortedNames)
         {
             throw new NotImplementedException();
         }
     }
}
