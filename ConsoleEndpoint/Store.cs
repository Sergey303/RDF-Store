namespace ConsoleEndpoint
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;

    using ConsoleEndpoint.Comparers;
    using ConsoleEndpoint.Interface;

    using Polar.CellIndexes;

    /// Хранилище
    public class Store : IStore
    {
        // Есть таблица имен для хранения строк IRI
        private INametable nametable;

        public TableView table;

        // Индексы
        private IndexDynamic<spo, IndexViewImmutable<spo>> index_spo;

        private IndexDynamic<ops, IndexViewImmutable<ops>> index_ops;

        private IndexDynamic<int, IndexViewImmutable<int>> index_p;

        // Конструктор
        public Store(Stream tab_stream, Stream index1, Stream index2, Stream index3, INametable nametable)
        {
            this.nametable = nametable;
            this.table = new TableView(
                tab_stream,
                PolarExtension.GetPolarType<IRecord<int, int, IUnion<int, string>>>());

            IndexViewImmutable<spo> index_spo_i =
                new IndexViewImmutable<spo>(index1)
                    {
                        KeyProducer =
                            ob => new spo(
                                ob.CastRow<object, object>().Item2
                                    .CastRow<int, int, object[]>()),
                        Table = this.table,
                        Scale = null,
                        tosort = true
                    };
            this.index_spo = new IndexDynamic<spo, IndexViewImmutable<spo>>(true, index_spo_i);
            this.table.RegisterIndex(this.index_spo);

            IndexViewImmutable<ops> index_ops_i =
                new IndexViewImmutable<ops>(index2)
                    {
                        KeyProducer =
                            ob => new ops(
                                ob.CastRow<object, object>().Item2
                                    .CastRow<int, int, object[]>()),
                        Table = this.table,
                        Scale = null,
                        tosort = true
                    };
            this.index_ops = new IndexDynamic<ops, IndexViewImmutable<ops>>(true, index_ops_i);
            this.table.RegisterIndex(this.index_ops);

            IndexViewImmutable<int> index_p_i =
                new IndexViewImmutable<int>(index3)
                    {
                        KeyProducer =
                            ob => (int)ob.CastRow<object, object[]>().Item2[1],
                        Table = this.table,
                        Scale = null,
                        tosort = true
                    };
            this.index_p = new IndexDynamic<int, IndexViewImmutable<int>>(true, index_p_i);
            this.table.RegisterIndex(this.index_p);
        }

        public void Load(IEnumerable<object> tripleFlow)
        {
            this.table.Clear();
            this.table.ClearIndexes();
            var t = new Stopwatch();
            
            t.Restart();
            this.table.AddPortion(tripleFlow);
            t.Stop();
            Console.WriteLine("add " + t.ElapsedMilliseconds);

            t.Restart();
            this.table.BuildIndexes();
            t.Stop();
            Console.WriteLine("build " + t.ElapsedMilliseconds);
        }

        public IEnumerable<object[]> spO(int sCode, int pCode)
        {
            return this.index_spo.GetAllUndeletedByLevel(
                row =>
                    {
                        var (s1, p1, _) = row.CastRow<int, int, object>();
                        var comparisonS = s1.CompareTo(sCode);
                        return comparisonS != 0 ? comparisonS : p1.CompareTo(pCode);
                    });
        }

        public IEnumerable<object[]> sPo(int sCode, object[] oObjVariant)
        {
            return this.index_spo.GetAllUndeletedByLevel(
                row =>
                    {
                        var (s1, _, o1) = row.CastRow<int, object, object[]>();
                        var comparisonS = s1.CompareTo(sCode);
                        return comparisonS != 0 ? comparisonS : ObjectVariantComparer.Default.Compare(o1, oObjVariant);
                    });
        }

        public IEnumerable<object[]> Spo(int pCode, object[] oObjVariant)
        {
            return this.index_ops.GetAllUndeletedByLevel(
                row =>
                    {
                        var (_, p1, o1) = row.CastRow<object, int, object[]>();
                        var comparisonO = ObjectVariantComparer.Default.Compare(o1, oObjVariant);
                        return comparisonO != 0 ? comparisonO : p1.CompareTo(pCode);
                    });
        }

        public IEnumerable<object[]> SpO(int pCode)
        {
            return this.index_spo.GetAllUndeletedByLevel(
                row => row.CastRow<object, int, object>().Item2.CompareTo(pCode));
        }

        public IEnumerable<object[]> sPO(int sCode)
        {
            return this.index_spo.GetAllUndeletedByLevel(
                row => row.CastRow<int, object, object>().Item1.CompareTo(sCode));
        }

        public IEnumerable<object[]> SPo(object[] oObjVariant)
        {
            return this.index_ops.GetAllUndeletedByLevel(row => ObjectVariantComparer.Default.Compare(row.CastRow<object, object, object[]>().Item3, oObjVariant));
        }

        public IEnumerable<object[]> SPO()
        {
            return this.index_spo.GetAllUndeletedByLevel(objects => 0);
        }
        public bool Contains(int sCode, int pCode, object[] oObjVariant)
        {
            return this.index_spo.GetAllUndeletedByLevel(
                row =>
                    {
                        var (s1, p1, o1) = row.CastRow<int, int, object[]>();
                        var comparisonS = s1.CompareTo(sCode);
                        if (comparisonS != 0) return comparisonS;
                        else
                        {
                            var comparisonP = p1.CompareTo(pCode);
                            return comparisonP != 0 ? comparisonP : ObjectVariantComparer.Default.Compare(o1, oObjVariant);
                        }
                    }).Any();
        }
    }
}