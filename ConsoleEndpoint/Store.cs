namespace ConsoleEndpoint
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Polar.CellIndexes;

    /// Хранилище
    public class Store : IStore // - Слишком сложный интерфейс 
    {
        // Есть таблица имен для хранения строк IRI
        private INametable nametable; // пока не используется
        
        // Основная таблица - таблица триплетов
       public TableView table;
        // Индексы
        private IndexDynamic<spo, IndexViewImmutable<spo>> index_spo;
        private IndexDynamic<ops, IndexViewImmutable<ops>> index_ops;
        private IndexDynamic<int, IndexViewImmutable<int>> index_p;

        // Конструктор
        public Store(Stream tab_stream, Stream index1, Stream index2, Stream index3, INametable nametable)
        {
            this.nametable = nametable;
            this.table = new TableView(tab_stream, PolarExtension.GetPolarType<IRecord<int,int,IUnion<int,string>>>());
            
            IndexViewImmutable<spo> index_spo_i = new IndexViewImmutable<spo>(index1)
                                                      {
                                                          KeyProducer = ob => new spo(ob.CastRow<object,object>().Item2.CastRow<int,int,OV>()),
                                                          Table = this.table,
                                                          Scale = null,
                                                          tosort = true
                                                      };
            this.index_spo = new IndexDynamic<spo, IndexViewImmutable<spo>>(true, index_spo_i);
            this.table.RegisterIndex(this.index_spo);

            IndexViewImmutable<ops> index_ops_i = new IndexViewImmutable<ops>(index2)
                                                      {
                                                          KeyProducer = ob => new ops(ob.CastRow<object, object>().Item2.CastRow<int, int, OV>()),
                                                          Table = this.table,
                                                          Scale = null,
                                                          tosort = true
                                                      };
            this.index_ops = new IndexDynamic<ops, IndexViewImmutable<ops>>(true, index_ops_i);
            this.table.RegisterIndex(this.index_ops);
            
            IndexViewImmutable<int> index_p_i = new IndexViewImmutable<int>(index3)
                                                      {
                                                          KeyProducer = ob => (int)ob.CastRow<object, object[]>().Item2[1],
                                                          Table = this.table,
                                                          Scale = null,
                                                          tosort = true
                                                      };
            this.index_p = new IndexDynamic<int, IndexViewImmutable<int>>(true, index_p_i);
            this.table.RegisterIndex(this.index_p);
        }
        
        public void Load(IEnumerable<(string s, string p, OV o)> tripleFlow)
        {
            this.table.Clear();
            this.table.ClearIndexes();
            var t = new Stopwatch();
            t.Restart();
            var array = tripleFlow.Select(triple =>
                new object[] {
                                     this.nametable.GetSetCode(triple.s),
                                     this.nametable.GetSetCode(triple.p),
                                    new object[] { (int)triple.o.OVid, triple.o.OVid == OVT.iri ? this.nametable.GetSetCode((string)triple.o.OValue) : triple.o.OValue}
                                 }).ToArray();
            t.Stop();
            Console.WriteLine("cast 2 objects arrays " + t.ElapsedMilliseconds);

            t.Restart();
            this.table.AddPortion(array);
            t.Stop();
            Console.WriteLine("add " + t.ElapsedMilliseconds);

            t.Restart();
            this.table.BuildIndexes();
            t.Stop();
            Console.WriteLine("build " + t.ElapsedMilliseconds);
        }

       
        public IEnumerable<OV> spO(int s, int p) =>
            this.index_spo.GetAllUndeletedByLevel(row =>
                    {
                        var (s1, p1, _) = row.CastRow<int, int, object>();
                        var comparisonS =s1.CompareTo(s);
                        return comparisonS != 0 ? comparisonS : (p1).CompareTo(p);
                    })
                .Select(row => row[2])
                .CastAllRows<OV>()
                .ToArray();

        public IEnumerable<int> sPo(int s, OV o) =>
            this.index_spo.GetAllUndeletedByLevel(row =>
                    {
                        var (s1, _, o1) = row.CastRow<int, object, OV>();
                        var comparisonS = s1.CompareTo(s);
                        return comparisonS != 0 ? comparisonS : o1.CompareTo(o);
                    }).Select(record => (int)record[1]);

        public IEnumerable<int> Spo(int p, OV o) =>
            this.index_ops.GetAllUndeletedByLevel(row =>
                {
                    var (_, p1, o1) = row.CastRow<object, int, OV>();
                    var comparisonO = o1.CompareTo(o);
                    return comparisonO != 0 ? comparisonO : p1.CompareTo(p);
                }).Select(row1 => (int)row1[0]);

        public IEnumerable<(int s, OV o)> SpO(int p)
        {
            foreach (var row in this.index_spo.GetAllUndeletedByLevel(row => row.CastRow<object,int, object>().Item2.CompareTo(p)))
            {
                var (s, _, o) = row.CastRow<int, object, OV>();
                yield return (s, o);
            }
        }

        public IEnumerable<(int p, OV o)> sPO(int subject)
        {
            foreach (var row in this.index_spo.GetAllUndeletedByLevel(row => row.CastRow<int,object, object>().Item1.CompareTo(subject)))
            {
                var (_, p, o) = row.CastRow<object, int, OV>();
                yield return (p, o);
            }
        }

        public IEnumerable<(int s, int p)> SPo(OV o)
        {
            return this.index_ops.GetAllUndeletedByLevel(row => row.CastRow<object, object, OV>().Item3.CompareTo(o))
               .CastAllRows<(int s, int p, object o)>()
               .Select(row => (row.s, row.p))
               .ToArray();
        }

        public IEnumerable<(int s, int p, OV o)> SPO()
        {
          return this.table.TableCell.Root.ElementValues()
               .CastAllRows<(bool deleted, object row)>()
               .Where(record => !record.deleted)
               .Select(record => record.row)
               .CastAllRows<(int s, int p, OV o)>()
               .ToArray();
        }
        public bool Contains(int s, int p, OV o)
        {
            return this.index_spo.GetAllUndeletedByLevel(row =>
                    {
                      var  (s1, p1, o1) = row.CastRow<int, int, OV>();
                        var comparisonS = s1.CompareTo(s);
                        if (comparisonS != 0) return comparisonS;
                        else
                        {
                            var comparisonP = p1.CompareTo(p);
                            return comparisonP != 0 ? comparisonP : o1.CompareTo(o);
                        }
                    })
                .Any();
        }

    }
}