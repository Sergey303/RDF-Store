using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polar.CellIndexes;
using Polar.DB;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using RDFStore.CascadingIndex;
using RDFTurtleParser;
using IBearingTableImmutable = Polar.CellIndexes.IBearingTableImmutable;

namespace RDFStore
{
    public class RDFGraph : IGraph
    {
        public readonly string Path;

        public TableView table;

        private IndexCascadingDynamic<ObjectVariants> po_index;

        private IndexCascadingDynamic<int> ps_index;
        private TextObjectIndex textObjectIndex;
        private readonly NodeGeneratorInt _ng;
        private const int portionOfTriplesToLoad = 3000 * 1000;

        private event Action<IEnumerable<TableRow>> OnAddPortion;

        public RDFGraph(string path)
        {
            Path = path;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            PType tp_triple = new PTypeRecord(
                new NamedType("subj", new PType(PTypeEnumeration.integer)),
                new NamedType("pred", new PType(PTypeEnumeration.integer)),
                new NamedType("obj", ObjectVariantsPolarType.ObjectVariantPolarType));
            table = new TableView(path + "triples", tp_triple);
            ps_index = new IndexCascadingDynamic<int>(path + "ps_index",
              (IBearingTableImmutable)table,
                ob => (int)((object[])((object[])ob)[1])[1],
                ob => (int)((object[])((object[])ob)[1])[0],
                i => i);
            po_index = new IndexCascadingDynamic<ObjectVariants>(path + "po_index",
                (IBearingTableImmutable)table,
                ob => (int)((object[])((object[])ob)[1])[1],
                ob => ((object[])((object[])ob)[1])[2].ToOVariant(NodeGenerator),
                ov => ov.GetHashCode());
            textObjectIndex=new TextObjectIndex((ulong) (table.TableCell.IsEmpty ? 10 : table.TableCell.Root.Count()/3+1), this);
            OnAddPortion += po_index.index_arr.FillPortion;
            OnAddPortion += ps_index.index_arr.FillPortion;
            OnAddPortion += textObjectIndex.FillPortion;
            NodeGenerator = NodeGeneratorInt.Create(new NameTableDictionaryRam());//new NametableLinearBuffered(path));
            _ng = NodeGenerator as NodeGeneratorInt;
            _ng.coding_table.Expand((int)10000/3+1, Enumerable.Repeat(SpecialTypesClass.RdfType, 1));           
        }

        public string Name { get; set; }

        public NodeGenerator NodeGenerator { get; set; }
        public TableView Table { get { return table; } }

        public void ActivateCache()
        {
            (_ng.coding_table as NametableLinearBuffered)?.ActivateCache();
          //  table.ActivateCache();
            ps_index.ActivateCache();
            po_index.ActivateCache();
        }

        public void Add(ObjectVariants s, ObjectVariants p, ObjectVariants o)
        {
            if(s.Variant==ObjectVariantEnum.Iri)
                s=_ng.GetUri(s.Content);
            if (p.Variant == ObjectVariantEnum.Iri)
                p = _ng.GetUri(p.Content);
            if (o.Variant == ObjectVariantEnum.Iri)
                o = _ng.GetUri(o.Content);
            OnAddPortion(table.Add(new []{new []{s, p, o}}).ToArray());
        }

        public bool Any()
        {
            return table.Elements().Any(row => !(bool)row.Field(0).Get());
        }


        
        public void AddTriples(long tripletsCount, IGenerator<List<TripleStrOV>> generator)
        {
          //  textObjectIndex=new TextObjectIndex((ulong) (tripletsCount/3)+1, this);
          
            generator.Start(ProcessPortion);
            table.TableCell.Flush();
           

            po_index.index_arr.FillFinish();
            ps_index.index_arr.FillFinish();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
        }

        public void BuildIndexes()
        {
            
            _ng.coding_table.Save();
            _ng.coding_table.FreeMemory();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            if (!table.TableCell.Root.Elements().Any())
            {
                Console.WriteLine("table empty!");
                return;
            }

            sw.Stop();
            Console.WriteLine("Load data and nametable ok. Duration={0}", sw.ElapsedMilliseconds);
            sw.Restart();

            //ng.Build();
            ps_index.Build();

            sw.Stop();
            Console.WriteLine("ps_index.Build() ok. Duration={0}", sw.ElapsedMilliseconds);
            sw.Restart();

            po_index.Build();

            sw.Stop();
            Console.WriteLine("Build index ok. Duration={0}", sw.ElapsedMilliseconds);
            sw.Restart();

            NodeGenerator.Build();
        }
        
        [Obsolete]
        public void Build(long nodesCount, IEnumerable<TripleStrOV> triples)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
           

            po_index.index_arr.FillInit();
            ps_index.index_arr.FillInit();
            table.Clear();
            table.Fill(new object[0]);
            var ng = NodeGenerator as NodeGeneratorInt;
            ng.coding_table.Expand((int)nodesCount, Enumerable.Repeat(SpecialTypesClass.RdfType, 1));
            //((NameTableUniversal)ng.coding_table).BuildIndexes();
            //((NameTableUniversal)ng.coding_table).BuildScale();
            List<TripleStrOV> buff = new List<TripleStrOV>();
            foreach (TripleStrOV tri in triples)
            {
                buff.Add(tri);
                if (buff.Count >= portionOfTriplesToLoad)
                {
                    ProcessPortion(buff);
                    Console.WriteLine("portion ok ");
                    buff.Clear();
                }
            }
            if (buff.Count > 0) ProcessPortion(buff);
            table.TableCell.Flush();
            ng.coding_table.Save();

          

            po_index.index_arr.FillFinish();
            ps_index.index_arr.FillFinish();

            ng.coding_table.FreeMemory();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            sw.Stop();
            Console.WriteLine("Load data and nametable ok. Duration={0}", sw.ElapsedMilliseconds);
            sw.Restart();

            ps_index.Build();

            sw.Stop();
            Console.WriteLine("ps_index.Build() ok. Duration={0}", sw.ElapsedMilliseconds);
            sw.Restart();

            po_index.Build();

            sw.Stop();
            Console.WriteLine("Build index ok. Duration={0}", sw.ElapsedMilliseconds);
            sw.Restart();
        }

        public void Clear()
        {
            table.Clear();
            table.Fill(new object[0]);
            var ng = NodeGenerator as NodeGeneratorInt;
            ng.Clear();
            ps_index.DropIndex();
            po_index.DropIndex();
        }


        public void Delete(ObjectVariants subject, ObjectVariants predicate, ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public void AddFromTurtle(long iri_Count, string gString)
        {
            AddTriples(iri_Count, new TripleGeneratorBuffered(gString, null, portionOfTriplesToLoad));
        }


        public void FromTurtle(long iri_Count, string gString)
        {
            AddTriples(iri_Count, new TripleGeneratorBuffered(gString, null, portionOfTriplesToLoad));
            
        }

        public void FromTurtle(string fullName)
        {
            AddTriples(1000*1000, new TripleGeneratorBufferedParallel(fullName, null));
        }

        public void FromTurtle(long iri_Count, Stream inputStream)
        {
            AddTriples(iri_Count, new TripleGeneratorBufferedParallel(inputStream, null));
         
        }

        #region Get triples

        public bool Contains(ObjectVariants subj, ObjectVariants pred, ObjectVariants obj)
        {
            return GetTriplesWithSubjectPredicate(subj, pred)
                .Any(o => o.CompareTo(obj) == 0);
        }

        public IEnumerable<ObjectVariants> GetAllSubjects()
        {
            return
                ps_index.GetRecordsAll()
                    .Cast<object[]>()
                    .Select(row => (int) row[0])
                    .Distinct()
                    .Select(uri => NodeGenerator.GetUri(uri));
        }

        public IEnumerable<ObjectVariants> GetSubjects(ObjectVariants pred, ObjectVariants obj)
        {
            return po_index.GetRecordsWithKeys(((OV_iriint) pred).code, obj)
                .Cast<object[]>()

                .Select(rec => NodeGenerator.GetUri(rec[0]));
        }

        public IEnumerable<T> GetTriples<T>(Func<ObjectVariants, ObjectVariants, ObjectVariants, T> returns)
        {
            if (Table.TableCell.Root.Count() == 0) return Enumerable.Empty<T>();
            return ps_index.GetRecordsAll()
                .Cast<object[]>()
                .Select(
                    rec =>
                        returns(NodeGenerator.GetUri(rec[0]), NodeGenerator.GetUri(rec[1]),
                            rec[2].ToOVariant(NodeGenerator)));
        }

        public IEnumerable<T> GetTriplesCoded<T>(Func<int, int, ObjectVariants, T> returns)
        {
            if (Table.TableCell.Root.Count() == 0) return Enumerable.Empty<T>();
            return ps_index.GetRecordsAll()
                .Cast<object[]>()
                .Select(rec => returns((int) rec[0], (int) rec[1], rec[2].ToOVariant(NodeGenerator)));
        }

        public long GetTriplesCount()
        {
            return table.Elements().Count(row => !(bool) row.Field(0).Get());
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithTextObject(ObjectVariants obj)
        {
            var tableRow = Table.Element(0);
            return textObjectIndex.FindText(obj.ToString())
                .Select(offset =>
                {
                    tableRow.offset = offset;
                    return tableRow.Get();
                })
                .Cast<object[]>()
                .Select(rec => new TripleOVStruct(NodeGenerator.GetUri(rec[0]), NodeGenerator.GetUri(rec[1]), null));
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithObject(ObjectVariants obj)
        {
            return po_index.GetRecordsWithKey2(obj)
                .Cast<object[]>()
                .Select(rec => new TripleOVStruct(NodeGenerator.GetUri(rec[0]), NodeGenerator.GetUri(rec[1]), null));
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithPredicate(ObjectVariants pred)
        {
            return ps_index.GetRecordsWithKey1(((OV_iriint) pred).code)
                .Cast<object[]>()
                .Select(rec => new TripleOVStruct(NodeGenerator.GetUri(rec[0]), null, rec[2].ToOVariant(NodeGenerator)));
        }

        public IEnumerable<TripleOVStruct> GetTriplesWithSubject(ObjectVariants subj)
        {
            return ps_index.GetRecordsWithKey2(((OV_iriint) subj).code)
                .Cast<object[]>()

                .Select(rec => new TripleOVStruct(null, NodeGenerator.GetUri(rec[1]), rec[2].ToOVariant(NodeGenerator)));
        }

        public IEnumerable<ObjectVariants> GetTriplesWithSubjectObject(ObjectVariants subj, ObjectVariants obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ObjectVariants> GetTriplesWithSubjectPredicate(ObjectVariants subj, ObjectVariants pred)
        {
            return ps_index.GetRecordsWithKeys(((OV_iriint) pred).code, ((OV_iriint) subj).code)
                .Cast<object[]>()

                .Select(rec => rec[2].ToOVariant(NodeGenerator));
        }

        #endregion

        public void Start()
        {
            ps_index.CreateDiscaleDictionary();
            po_index.CreateDiscaleDictionary();
        }

        public void Warmup()
        {
            table.Warmup();
        }

        public void FromTurtle(Stream requestInputStream)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<TripleOV> enumerable)
        {
            OnAddPortion(table.Add(enumerable.Select(ov =>
            {
                var objects = new object[]
                {
                    ov.Subject.Variant==ObjectVariantEnum.Iri ? _ng.AddIri((string) ov.Subject.Content).WritableValue : ov.Subject.WritableValue,
                    ov.Predicate.Variant==ObjectVariantEnum.Iri ? _ng.AddIri((string) ov.Predicate.Content).WritableValue: ov.Predicate.WritableValue,
                    ov.Object.Variant==ObjectVariantEnum.Iri ? _ng.AddIri((string) ov.Object.Content).ToWritable() : ov.Object.ToWritable()
                };
                return objects;
            })).ToArray());
            table.TableCell.Flush();


            po_index.index_arr.FillFinish();
            ps_index.index_arr.FillFinish();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            BuildIndexes();
        }

        public void Add(IEnumerable<TripleStrOV> selectMany)
        {
            ProcessPortion(selectMany.ToList());
        }

        private void ProcessPortion(List<TripleStrOV> buff)
        {
            // Пополнение таблицы имен
          
            Console.WriteLine("portion readed");
            var portionCodedTriples = CodePortionTriples(buff);

            OnAddPortion( table.Add(portionCodedTriples).ToArray());
            Console.WriteLine("portion writed");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private IEnumerable<object[]> CodePortionTriples(List<TripleStrOV> buff)
        {
            var dic = _ng.coding_table.InsertPortion(buff.SelectMany(t =>
            {
                ObjectVariants ov = t.Object;
                //if (ov == null)
                //{
                //    Console.WriteLine(t.Subject);
                //    return Enumerable.Empty<string>();
                //}
                //else
                if (ov.Variant == ObjectVariantEnum.Iri)
                {
                    return new string[] {t.Subject, t.Predicate, ((OV_iri) ov).Name};
                }
                else
                {
                    return new string[] {t.Subject, t.Predicate};
                }
            }));

            return buff.Where(t => t.Object != null).Select(t =>
            {
                ObjectVariants ov = t.Object;
                if (ov.Variant == ObjectVariantEnum.Iri)
                    ov = new OV_iriint(dic[((OV_iri) ov).Name], _ng.coding_table.GetString);
                return new object[] { dic[t.Subject], dic[t.Predicate], ov.ToWritable()};
            });
        }
    }
}