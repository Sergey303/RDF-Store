using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RDFCommon;
using RDFCommon.Interfaces;

namespace RDFStore
{
    public class NameTableDictionaryRam: INametable, IGetDictionaryLong<string, int>
    {
        private readonly Dictionary<string, int> _code=new Dictionary<string, int>();
        private readonly List<string> _decode;
        private readonly string _decodeFilePath;
        private int _countSavedStrings = 0;

        public NameTableDictionaryRam(): this(Config.DatabaseFolder) { }
        public NameTableDictionaryRam(string decodeFileDirectoryPath)
        {
            _decodeFilePath = decodeFileDirectoryPath + "name table strings.txt";
            if (!File.Exists(_decodeFilePath))
            {
                File.Create(_decodeFilePath).Close();
            }
            _decode = File.ReadAllLines(_decodeFilePath).ToList();
            _countSavedStrings = _decode.Count;
            for (int i = 0; i < _countSavedStrings; i++)
            {
                _code.Add(_decode[i], i);
            }
        }
        public long LongCount() { return _decode.Count; }
        public void Clear()
        {
            _code.Clear();
            _decode.Clear();
            File.Delete(_decodeFilePath);
            _countSavedStrings = 0;
        }

        public int Capacity { get; set; }
        public int GetCode(string s)
        {
            return _code.TryGetValue(s, out var c) ? c : int.MinValue;
        }

        public string GetString(int c)
        {
            return 0 <= c && _decode.Count > c ? _decode[c] : null;
        }

        public int GetSetCode(string s)
        {
            int c;
            if (!_code.TryGetValue(s, out c))
            {
                _code.Add(s, c=_decode.Count);
                _decode.Add(s);
            }
            return c;
        }

        public void Expand(int length_estimation, IEnumerable<string> keyflow)
        {
            InsertPortion(keyflow);
        }

        public void Save()
        {
            using (StreamWriter writer=new StreamWriter(_decodeFilePath))
            {
                while (_countSavedStrings<_decode.Count)
                {
                    writer.WriteLine(_decode[_countSavedStrings++]);
                }
            }
        }

        public int this[string key]
        {
            get { return _code[key]; }
        }

        public IGetLongStream<string> Keys { get; }
        public IGetLongStream<int> Values { get; }
        public bool ContainsKey(string key)
        {
            return ContainsKey(key);
        }

     

        public bool TryGetValue(string key, out int value)
        {
            return _code.TryGetValue(key, out value);
        }

        public void FreeMemory()
        {
           
        }

        public ulong Count { get; }

        public Dictionary<string, int> InsertPortion(IEnumerable<string> unsorted)
        {
            throw new NotImplementedException();
            foreach (var s in unsorted)
            {
                GetSetCode(s);
            }
            return null;
        }

        public int EmptyCode { get { return Int32.MinValue; } }
    }
}
