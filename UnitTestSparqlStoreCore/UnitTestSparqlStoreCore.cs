using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFCommon;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using RDFStore;

namespace UnitTestSparqlStoreCore
{
    [TestClass]
    public class UnitTestSparqlStoreCore
    {
        private Store _store;
        private string _dataDirectory;

        [ClassInitialize]
        public void TestLoad()
        {
            var turtleFilePath = @"..\..\..\..\" + Config.TurtleFileFullPath;
            _dataDirectory = @"..\..\..\..\" + Config.DatabaseFolder;
            //var store = new RDFRamStore(); 
            _dataDirectory = @"..\..\..\" + Config.DatabaseFolder;
            _store = new Store(_dataDirectory);
            _store.ReloadFrom(turtleFilePath);
            //store.BuildIndexes();
        }

        [TestMethod]
        public void TestSimpleTable()
        {

            Assert.AreEqual(12, _store.GetTriplesCount());

            Assert.IsTrue(_store.Any());

            var id1IriString = new OV_iri("id1");
            var nameIriString = new OV_iri("name");
            var typeIriString = new OV_iri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
            // получаем коды и iri_Coded
            Assert.IsTrue(_store.NodeGenerator.TryGetUri(id1IriString, out var id1));
            Assert.IsTrue(_store.NodeGenerator.TryGetUri(nameIriString, out var name));
            Assert.IsTrue(_store.NodeGenerator.TryGetUri(typeIriString, out var type));

            var triplesWithSubjectPredicate = _store.GetTriplesWithSubjectPredicate(id1, name);
            Assert.AreEqual(1, triplesWithSubjectPredicate.Count());
            var literalRussia = triplesWithSubjectPredicate.First();
            Assert.AreEqual(new OV_string("Россия") ,literalRussia);

            Assert.IsTrue(_store.Contains(id1, name, literalRussia));
            Assert.IsFalse(_store.Contains(name, id1, literalRussia));


            var id1List = _store.GetTriplesWithPredicateObject(name, literalRussia).ToList();
            Assert.AreEqual(1 , id1List.Count);
            Assert.AreEqual(id1, id1List.First());

            var nameList = _store.GetTriplesWithSubjectObject(id1, literalRussia).ToList();
            Assert.AreEqual(1, nameList.Count);
            Assert.AreEqual(name, nameList.First());
            
            var triplesWithId1Subject = _store.GetTriplesWithSubject(id1).ToList();
            Assert.AreEqual(2, triplesWithId1Subject.Count);
            var triplesWithWrongSubject = _store.GetTriplesWithSubject(literalRussia).ToList();
            Assert.AreEqual(0, triplesWithWrongSubject.Count);

            var triplesWithType = _store.GetTriplesWithPredicate(type).ToList();
            Assert.AreEqual(5, triplesWithType.Count);
            var triplesWithWrongPredicate = _store.GetTriplesWithPredicate(id1).ToList();
            Assert.AreEqual(0, triplesWithWrongPredicate.Count);

            var triplesWithRussia = _store.GetTriplesWithObject(literalRussia).ToList();
            Assert.AreEqual(1, triplesWithType.Count);
            var id1NameRussia = triplesWithType.First();
            Assert.AreEqual(id1,id1NameRussia.Subject);
            Assert.AreEqual(name,id1NameRussia.Predicate);
            Assert.AreEqual(literalRussia,id1NameRussia.Object);
            var triplesWithId1Object = _store.GetTriplesWithObject(id1).ToList();
            Assert.AreEqual(1, triplesWithId1Object.Count);
            var tripleWithId1Obj = triplesWithId1Object.First();
            Assert.AreEqual(id1, tripleWithId1Obj.Subject);
            
        }
        [ClassCleanup]
        public void TestEndingCleanup()
        {
            if (Directory.Exists(_dataDirectory))
                Directory.Delete(_dataDirectory, true);
        }
    }
}
