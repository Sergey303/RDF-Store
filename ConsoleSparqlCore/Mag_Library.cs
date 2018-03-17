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
        public int EmptyCode { get { return Int32.MinValue; } }
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
            return EmptyCode;
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

            T.Restart();
            unknown = 0;
            int maxcode = (int)nt.LongCount() * 11 / 10; // лишние для тестирования отсутствующих кодов
            for (int j = 0; j < nprobes; j++)
            {
                int code = rnd.Next(maxcode);
                string scode = nt.GetString(code);
                if (scode == null) unknown++;
            }
            T.Stop();
            Console.WriteLine($" get {nprobes} string from codes. time {T.ElapsedMilliseconds}. {unknown} are unknown");

        }
    }

    // Класс объектных вариантов: или new object[] {1, iri} или object[] {2, "stroka"}
    public struct OV : IComparable
    {
        private object ob;
        public OV(object o) { ob = o; }
        public int CompareTo(object obj)
        {
            OV ov2 = (OV)obj;
            int t1 = (int)((object[])ob)[0];
            int t2 = (int)((object[])ov2.ob)[0];
            int cmp1 = t1.CompareTo(t2);
            if (cmp1 != 0) return cmp1;
            if (t1 == 1)
            {
                var v1 = (int)((object[])ob)[1];
                var v2 = (int)((object[])(ov2.ob))[1];
                return v1.CompareTo(v2);
            }
            else if (t1 == 2)
            {
                return ((string)((object[])ob)[1]).CompareTo((string)((object[])ov2.ob)[1]);
            }
            else throw new Exception("Err: 2988743");
        }
    }
    // Класс триплетов (объектное представление): object[] {subj, pred, obj} Все поля - OV.
    public class spo : IComparable
    {
        private int s, p; 
        private OV o;
        public spo(object ob) { object[] tri = (object[])ob; s = (int)tri[0]; p = (int)tri[1]; o = new OV(tri[2]); } 
        // В этом представлении сравнение идет по приоритету s, p, o
        public int CompareTo(object obj)
        {
            spo tr2 = (spo)obj;
            int s2 = tr2.s, p2 = tr2.p;
            OV o2 = tr2.o;
            
            int cmp1 = s.CompareTo(s2);
            if (cmp1 != 0) return cmp1;
            int cmp2 = p.CompareTo(p2);
            if (cmp2 != 0) return cmp2;
            return o.CompareTo(o2);
        }
    }
    // другой Класс триплетов (объектное представление): object[] {subj, pred, obj} Все поля - OV.
    public class ops : IComparable
    {
        private int s, p;
        private OV o;
        public ops(object ob) { object[] tri = (object[])ob; s = (int)tri[0]; p = (int)tri[1]; o = new OV(tri[2]); }
        // В этом представлении сравнение идет по приоритету o, p, s
        public int CompareTo(object obj)
        {
            ops tr2 = (ops)obj;
            int s2 = tr2.s, p2 = tr2.p;
            OV o2 = tr2.o;

            int cmp1 = o.CompareTo(o2);
            if (cmp1 != 0) return cmp1;
            int cmp2 = p.CompareTo(p2);
            if (cmp2 != 0) return cmp2;
            return s.CompareTo(s2);
        }
    }


    // Хранилище
    public class Mag_Store //: IStore // - Слишком сложный интерфейс 
    {
        // Есть таблица имен для хранения строк IRI
        private Mag_Nametable nametable; // пока не используется

        // Тип Object Variants
        PType tp_ov = new PTypeUnion(
            new NamedType("dummy", new PType(PTypeEnumeration.none)),
            new NamedType("iri", new PType(PTypeEnumeration.integer)),
            new NamedType("str", new PType(PTypeEnumeration.sstring)));
        // Тип триплепта
        PType tp_triple;
        // Основная таблица - таблица триплетов
        TableView table;
        // Индексы
        IndexDynamic<spo, IndexViewImmutable<spo>> index_spo;
        IndexDynamic<ops, IndexViewImmutable<ops>> index_ops;

        // Конструктор
        public Mag_Store(Stream tab_stream, Stream index1, Stream index2)
        {
            tp_triple = new PTypeRecord(
                //new NamedType("id", new PType(PTypeEnumeration.integer)), // Возможно, это временное решение
                new NamedType("subj", new PType(PTypeEnumeration.integer)),
                new NamedType("pred", new PType(PTypeEnumeration.integer)),
                new NamedType("obj", tp_ov));
            table = new TableView(tab_stream, tp_triple);
            IndexViewImmutable<spo> index_spo_i = new IndexViewImmutable<spo>(index1)
            {
                KeyProducer = ob => new spo(((object[])ob)[1]),
                Table = table,
                Scale = null
            };
            index_spo = new IndexDynamic<spo, IndexViewImmutable<spo>>(true, index_spo_i);
            table.RegisterIndex(index_spo);
            IndexViewImmutable<ops> index_ops_i = new IndexViewImmutable<ops>(index2)
            {
                KeyProducer = ob => new ops(((object[])ob)[1]),
                Table = table,
                Scale = null
            };
            index_ops = new IndexDynamic<ops, IndexViewImmutable<ops>>(true, index_ops_i);
            table.RegisterIndex(index_ops);
        }

        public void Load(IEnumerable<object> triples)
        {
            table.Clear();
            table.ClearIndexes();
            table.AddPortion(triples);
            table.BuildIndexes();
        }

        public IEnumerable<object> GetTriplesBySubject(int subj)
        {
            var q = index_spo.GetAllByLevel((PaEntry ent) =>
            {
                object[] rec = (object[])((object[])((object[])ent.Get()))[1];
                return ((int)rec[0]).CompareTo(subj);
            }).Select(ent => ((object[])ent.Get())[1])
            .ToArray();
            return q;
        }
    }

}
