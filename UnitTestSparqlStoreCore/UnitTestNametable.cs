using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleSparqlCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using RDFStore;

namespace UnitTestSparqlStoreCore
{
    [TestClass]
    public class UnitTestNameTable
    {
        private static IStore _store;
        private static string _dataDirectory;
        private static INametable _magnt;
        private static FileStream _fileStream1;
        private static FileStream _fileStream2;

        [ClassInitialize]
        public static void TestLoad(TestContext context)
        {
           var path = "mag_data/";
            _fileStream1 = File.Open(path + "name table ids.pac", FileMode.OpenOrCreate);
            _fileStream2 = File.Open(path + "name table off.pac", FileMode.OpenOrCreate);
            _magnt = new Mag_Nametable(_fileStream1, _fileStream2);
        }

        [TestMethod]
        public void TestNameTable()
        {

            Random rnd = new Random();
            string prefix = "PupkinQQUTEUJSHJDHJGFHSGDHWTYTHXCGHGCHSHSDYSTDSHSGHSG_";
            int max = 1_000_000;
            int total = 7 * max;
            List<string> names =  Enumerable.Range(0, total).Select(i=> prefix+rnd.Next(max)).ToList();
            List<int> codes =new List<int>();

            total++;
            names.Add(string.Empty);

            for (int i = 0; i < total; i++)
            {
                string scode = prefix + rnd.Next(max);
                var code = _magnt.GetSetCode(scode);
            }
          
            int nprobes = 1_000_000;

             int unknown = 0;
            for (int j = 0; j < nprobes; j++)
            {
                string scode = prefix + rnd.Next(max);
                int code = _magnt.GetCode(scode);
                if (code == Int32.MinValue) unknown++;
            }
            unknown = 0;
            int maxcode = (int)_magnt.LongCount() * 11 / 10; // лишние для тестирования отсутствующих кодов
            for (int j = 0; j < nprobes; j++)
            {
                int code = rnd.Next(maxcode);
                string scode = _magnt.GetString(code);
                if (scode == null) unknown++;
            }
         }
        [ClassCleanup]
        public static void TestEndingCleanup()
        {
            _fileStream1.Close();
            _fileStream2.Close();
        }
    }
}
