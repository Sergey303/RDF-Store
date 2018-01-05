using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Polar.Cells;
using Polar.DB;
using RDFCommon.Hashes;
using RDFCommon.Interfaces;
using RDFCommon.long_dictionary;

namespace RDFStore
{
    public class NametableLinearBuffered :  INametable
    {
        private readonly PaCell cell_keys;
        private readonly PaCell cell_f2_offsets; // Последовательность пар целых значений: f2, code
        public readonly PaCell cell_codes;
        public uint arrLength;
        private double factor = 2.0;
        public BitArray positionUsed;
        private readonly string path;
        private PaEntry search_key = PaEntry.Empty;
        private bool saved = true;
        private int maxSearchPortion = 8*1024*1024;
        private ulong bufferCapacity;
        private Buffer4Nametable buffer;

        #region  locals

        private readonly List<int> tmpCodesList = new List<int>();
        private readonly Dictionary<string, Tuple<KeyValueHash<string, int, ulong>, int>> writePosition = new Dictionary<string, Tuple<KeyValueHash<string, int, ulong>, int>>();
        private readonly List<KeyValuePair<int, int>> sections = new List<KeyValuePair<int, int>>();
        private readonly HashSet<int> f2Hashes = new HashSet<int>();

        #endregion


        private int F1(ulong h)
        {
            return Convert.ToInt32( h%arrLength);
        }

        private int F2(string s)
        {
            return (int)s.GetHashModifiedBernstein();
        }

        public NametableLinearBuffered(string path, ulong buffer_size= 2 * 1000 * 1000)
        {
            this.path = path;

            //keys = new List<string>();
            // arr = new KeyNode[1000];
            //Clear();
            cell_keys = new PaCell(new PTypeSequence(new PType(PTypeEnumeration.sstring)), path + "cell_keys.pac", false);
            cell_f2_offsets =
                new PaCell(new PTypeSequence(new PTypeRecord(new NamedType("f2", new PType(PTypeEnumeration.integer)),
                    new NamedType("offset", new PType(PTypeEnumeration.longinteger)))), path + "cell_keys_offsets.pac",
                    false);
            cell_codes = new PaCell(new PTypeSequence(new PType(PTypeEnumeration.integer)), path + "cell codes.pac",
                false);
            buffer=new Buffer4Nametable(s => (ulong) F2(s), buffer_size, buffer_size);
            bufferCapacity = buffer_size;
            if (!cell_keys.IsEmpty)
            {
                //todo repair other files
                arrLength = (uint) cell_codes.Root.Count();
                var pathOfBits = path + "bits of used positions.bit";
                positionUsed = File.Exists(pathOfBits)
                    ? new BitArray(File.ReadAllBytes(pathOfBits))
                    : new BitArray(cell_codes.Root.ElementValues()
                        .Cast<int>()
                        .Select(el => el > -1)
                        .ToArray());
            }
        }

        public void WharmUp()
        {
            foreach (var p in cell_keys.Root.ElementValues()) ;
            foreach (var p in cell_f2_offsets.Root.ElementValues()) ;
            foreach (var p in cell_codes.Root.ElementValues()) ;
        }
        public void ActivateCache()
        {
            cell_keys.ActivateCache();
            cell_f2_offsets.ActivateCache();
            cell_codes.ActivateCache();
        }

        public void Expand(int length_estimation, IEnumerable<string> keyflow)
        {
            Clear();
            cell_codes.Clear();
            cell_codes.Fill(new object[0]);

            arrLength = (uint) (length_estimation*factor);
            arrLength = arrLength > 0 ? arrLength : int.MaxValue;
            positionUsed = new BitArray(Convert.ToInt32(arrLength));
          //todo long array bits

            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < arrLength; i++)
            {
                cell_codes.Root.AppendElement(-1);
            }
            cell_codes.Flush();
            timer.Stop();
            Console.WriteLine("create empty file of length " + arrLength + " time " + timer.Elapsed.TotalSeconds);
            int j = 0;
            foreach (string key in keyflow)
            {
                GetSetCode(key);
            }
            cell_keys.Flush();

        }

        public void Save()
        {
            if (saved) return;
            Console.WriteLine("save");
            WriteBuffer();
            GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
            var bytes = new byte[(positionUsed.Count - 1)/8 + 1];
            positionUsed.CopyTo(bytes, 0);

            File.WriteAllBytes(path + "bits of used positions.bit", bytes);
         
            saved = true;
        }

        public IGetDictionaryLong<string, int> InsertPortion(IEnumerable<string> unsorted)
        {
            if (buffer.Count >= bufferCapacity)
            {
             Save();
            }
            
            //BitArray positionUsedPreviousSearchedKey=new BitArray(Convert.ToInt32(arrLength));
            int codesCount = 0;
             foreach (var s in buffer.FilterExisting(unsorted
                .Where(s => s!=null)
                .Select(s=>new KeyValueHash<string, int, ulong>(s, s.GetHashSpooky()) {Value = -1} )
                .Where(s => !writePosition.ContainsKey(s.Key))))
             {
                 int f1 =  F1(s.Hash), start = f1;
                 if (!positionUsed[f1])
                 {
                     SetNewCode(s.Key, s.Hash);
                     continue;
                 }
                 f2Hashes.Add(F2(s.Key));
                 var firstFreePostion = f1;
                 while (true)
                 {
                     firstFreePostion++;
                     if (firstFreePostion == arrLength)
                     {
                         sections.Add(new KeyValuePair<int, int>(start, firstFreePostion));
                         codesCount += firstFreePostion - start;
                         start = firstFreePostion = 0;
                     }
                     if (firstFreePostion == f1)
                     {
                         writePosition.Add(s.Key, Tuple.Create(s, -1));
                         break;
                     }
                     if (positionUsed[firstFreePostion]) continue;
                     writePosition.Add(s.Key, Tuple.Create(s, firstFreePostion));
                     break;
                 }
                 var length = firstFreePostion - start;
                 if (length > 0)
                 {
                     sections.Add(new KeyValuePair<int, int>(start, firstFreePostion));
                     codesCount += length;
                 }
             }
            List<int> codesList = new List<int>(codesCount);
            int j = 0;
            BitArray readedPositions=new BitArray(Convert.ToInt32(arrLength));
            foreach (var pair in sections.OrderBy(pair => pair.Key))
            {
                for (int i = pair.Key; i < pair.Value; i++,j++)
                {
                    if (!positionUsed[i] || readedPositions[i]) continue;
                    codesList.Add( (int) cell_codes.Root.Element(i).Get());
                    readedPositions[i]=true;
                }
            }
            var codes = codesList.ToArray();
            Array.Sort(codes);

            if (search_key.IsEmpty && cell_keys.Root.Count() > 0) search_key = cell_keys.Root.Element(0);
         
            var codeOffsetToKeys = codesList.Select(code =>
                new {code, f2Offset = (object[]) cell_f2_offsets.Root.Element(code).Get()})
                .Where(t => f2Hashes.Contains((int) t.f2Offset[0]))
                .Select(t => new KeyValuePair<int, long>(t.code, (long) t.f2Offset[1]))
                .ToArray();
            foreach (var codeOffsetToKey in codeOffsetToKeys)
            {
                search_key.offset = codeOffsetToKey.Value;
                var s = (string) search_key.Get();
                Tuple<KeyValueHash<string, int, ulong>, int> wp;
                if (writePosition.TryGetValue(s, out wp))
                {
                    writePosition.Remove(s);
                  buffer.Inserted.Add(s, codeOffsetToKey.Key, wp.Item1.Hash );
                    if (writePosition.Count == 0) break;
                }
            }
            foreach (var key_position in writePosition.Values)
                SetNewCode(key_position.Item1.Key, key_position.Item1.Hash);

            writePosition.Clear();
            sections.Clear();
            f2Hashes.Clear();

            return buffer;
        }


        public void Clear()
        {
            buffer.Clear();
            cell_keys.Clear();
            cell_keys.Fill(new object[0]);
            cell_f2_offsets.Clear();
            cell_f2_offsets.Fill(new object[0]);
            positionUsed=new BitArray(Convert.ToInt32(arrLength));
            //for (int i = 0; i < arrLength; i++)
            //{
            //    cell_codes.Root.Element(i).Set(-1);
            //}
            saved = false;
        }

        public void FreeMemory()
        {
            Save();
            buffer=new Buffer4Nametable(s=>(ulong)s.GetHashSpooky(), 500, 500);
        }

        public int Capacity
        {
            get { return Convert.ToInt32(arrLength); }
            set { throw new NotImplementedException(); }
        }

        public int GetCode(string key)
        {
            return GetCode(new KeyValueHash<string, int, ulong>(key, key.GetHashSpooky()) { Value = -1}).Value;
        }

        public KeyValueHash<string, int, ulong> GetCode(KeyValueHash<string,int, ulong> keyValueHash)
        {
            keyValueHash = buffer.TryGetValue(keyValueHash);
            if (keyValueHash.HasValue)
                return keyValueHash;

            int ind = F1(keyValueHash.Hash);

            int f2 = F2(keyValueHash.Key);
            if (f2 == Int32.MinValue) f2 = 0;

            if (search_key.IsEmpty && cell_keys.Root.Count() > 0) search_key = cell_keys.Root.Element(0);

            int counter = ind;
            int lookedCount = 0;
            while (true)
            {
                if (!positionUsed[counter])
                    return keyValueHash;

                int searchToPosition = counter + 1;
                int limit = Math.Min(maxSearchPortion + counter, Convert.ToInt32(arrLength));
                while (searchToPosition < limit)
                {
                    if (!positionUsed[searchToPosition])
                        break;
                    searchToPosition++;
                }
                tmpCodesList.Clear();
                for (int i = counter; i < searchToPosition; i++)
                    {
                        var c = (int)cell_codes.Root.Element(i).Get();
                        if (c == -1) throw new Exception("used position hasn't code");
                        tmpCodesList.Add(c);
                    }

                var codes = tmpCodesList.ToArray();
                Array.Sort(codes);

                foreach (int code in codes)
                {
                    object[] hashOffset = (object[])(cell_f2_offsets.Root.Element(code)).Get();
                    var hash = (int)hashOffset[0];
                    if (hash == f2)
                    {
                        // Если найден, то выход, иначе это коллизия - надо продолжать.
                        // найдем offset, а потом вычислим ключ
                        var offset = (long)hashOffset[1];
                        search_key.offset = offset;
                        if ((string) search_key.Get() == keyValueHash.Key)
                        {
                            keyValueHash.Value = code;
                            keyValueHash.HasValue = true;
                            return keyValueHash;
                        }
                        Console.WriteLine("f2 collision");
                    }
                }
                lookedCount += searchToPosition - counter;
                counter = searchToPosition;
                if (counter >= arrLength)
                {
                    counter = 0;
                }
                if (lookedCount >= arrLength)
                    throw new Exception("Name table can't add new key.");
            }
        }

        public string GetString(int c)
        {
            if (c >= cell_f2_offsets.Root.Count())
                return buffer.NotSaved.Keys[Convert.ToUInt64(c)];
            // search in inserted?
            if (search_key.IsEmpty && cell_keys.Root.Count() > 0) search_key = cell_keys.Root.Element(0);
            search_key.offset = (long) cell_f2_offsets.Root.Element(c).Field(1).Get();
            return (string) search_key.Get();
        } 

        public int GetSetCode(string key)
        {
            if (arrLength == 0)
            {
                Expand(1, Enumerable.Repeat(key, 1));
                return 0;
            }

            var keyValueHash = new KeyValueHash<string, int, ulong> (key, key.GetHashSpooky()) { Value = -1};
            keyValueHash = GetCode(keyValueHash);
            if (keyValueHash.HasValue) return keyValueHash.Value;
            return SetNewCode(keyValueHash.Key, keyValueHash.Hash);
        }

        private int SetNewCode(string key, ulong hash)
        {
            int code = (int) Count;
          
            buffer.NotSaved.Add(key, code, hash);
                saved = false;
            if (buffer.NotSaved.Keys.Count >= bufferCapacity)
            {
                Save();
            }
            return code;
        }

        private void WriteBuffer()
        {
        // todo  if (arrLength > (ulong) cell_f2_offsets.Root.Count() + buffer.NotSaved.Count)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                var count = buffer.Count;
                var offsetHashs = new LongStream<Tuple<long, int>>(count);
                var positions = new LongStream<KeyValuePair<int, int>>(count);
                for (ulong i = 0; i < count; i++)
                {
                    var s = buffer.NotSaved.Keys[i];
                    offsetHashs[i] = Tuple.Create(cell_keys.Root.AppendElement(s), F2(s));
                    int p = F1(s.GetHashSpooky());
                    while (positionUsed[p])
                    {
                        p++;
                        if (p == arrLength) p = 0;
                    }
                    positionUsed[p] = true;
                    positions[i] = new KeyValuePair<int, int>(p, buffer.NotSaved.Values[i]);
                }
                cell_keys.Flush();
                for (ulong i = 0; i < offsetHashs.Length; i++)
                {
                    cell_f2_offsets.Root.AppendElement(new object[] {offsetHashs[i].Item2, offsetHashs[i].Item1});
                }
                cell_f2_offsets.Flush();
                foreach (var pair in positions.Sort(pair => pair.Key))
                {
                    cell_codes.Root.Element(pair.Key).Set(pair.Value);
                }
                cell_codes.Flush();

                buffer.OnSaved();

                timer.Stop();
                Console.WriteLine("write nametable buffer " + timer.Elapsed.TotalSeconds);

            }
            //else
            //{
            //    arrLength += arrLength;
            //    var newPositions=new BitArray(Convert.ToInt32(arrLength));
            //    cell_keys.Root.Scan((e, o) =>
            //    {
            //        new
            //    });
            //}
        }

        public long Count
        {
            get { ulong count = buffer.Count;
                long c2 = cell_f2_offsets.Root.Count();
                return (long)count + c2; }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                //todo check
                return buffer.Keys.Concat(cell_keys.Root.ElementValues().Cast<string>());
            }
        }
        public IEnumerable<int> Codes
        {
            get
            {
                //todo check
                for (int i = 0; i < Count; i++)
                    yield return i;
            }
        }

    }
}
  