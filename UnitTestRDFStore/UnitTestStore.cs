using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns.general;
using RDFStore;

namespace UnitTestRDFStore
{
    [TestClass]
    public class UnitTestStore
    {
        private static IStore _store;
        private static string _dataDirectory;

        [ClassInitialize()]
        public static void TestLoad(TestContext context)
        {
            var turtleFilePath = @"..\..\..\" + Config.TurtleFileFullPath; 
            _dataDirectory = @"..\..\..\" + Config.DatabaseFolder;
          // _store = new RDFRamStore(); 
            _store = new Store(_dataDirectory);
            _store.ReloadFrom(turtleFilePath);
           ((Store)_store).BuildIndexes();
        }

        [TestMethod]
        public void TestSimpleTable()
        {
                Assert.AreEqual(12, _store.GetTriplesCount());

                Assert.IsTrue(_store.Any());

                var id1IriString = new OV_iri("id1");
                var nameIriString = new OV_iri("name");
                var inOrgIriString = new OV_iri("in-org");
                var typeIriString = new OV_iri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
                // получаем коды и iri_Coded
                Assert.IsTrue(_store.NodeGenerator.TryGetUri(id1IriString, out var id1));
                Assert.IsTrue(_store.NodeGenerator.TryGetUri(nameIriString, out var name));
                Assert.IsTrue(_store.NodeGenerator.TryGetUri(typeIriString, out var type));
            Assert.IsTrue(_store.NodeGenerator.TryGetUri(inOrgIriString, out var inOrg));

            var triplesWithSubjectPredicate = _store.GetTriplesWithSubjectPredicate(id1, name);
                Assert.AreEqual(1, triplesWithSubjectPredicate.Count());
                var literalRussia = triplesWithSubjectPredicate.First();
                Assert.AreEqual(new OV_string("Россия"), literalRussia);

                Assert.IsTrue(_store.Contains(id1, name, literalRussia));
                Assert.IsFalse(_store.Contains(name, id1, literalRussia));


                var id1List = _store.GetTriplesWithPredicateObject(name, literalRussia).ToList();
                Assert.AreEqual(1, id1List.Count);
                Assert.AreEqual(id1, id1List.First());

                var nameList = _store.GetTriplesWithSubjectObject(id1, literalRussia).ToList();
                Assert.AreEqual(1, nameList.Count);
                Assert.AreEqual(name, nameList.First());

                var triplesWithId1Subject = _store.GetTriplesWithSubject(id1).ToList();
                Assert.AreEqual(2, triplesWithId1Subject.Count);
                var triplesWithWrongSubject = _store.GetTriplesWithSubject(literalRussia).ToList();
                Assert.AreEqual(0, triplesWithWrongSubject.Count);

                var triplesWithType = _store.GetTriplesWithPredicate(name).ToList();
                Assert.AreEqual(3, triplesWithType.Count);
                var triplesWithWrongPredicate = _store.GetTriplesWithPredicate(id1).ToList();
                Assert.AreEqual(0, triplesWithWrongPredicate.Count);

                var triplesWithRussia = _store.GetTriplesWithObject(literalRussia).ToList();
                Assert.AreEqual(1, triplesWithRussia.Count);
                var id1NameRussia = triplesWithRussia.First();
                Assert.AreEqual(id1, id1NameRussia.Subject);
                Assert.AreEqual(name, id1NameRussia.Predicate);
                Assert.AreEqual(literalRussia, id1NameRussia.Object);
                var triplesWithId1Object = _store.GetTriplesWithObject(id1).ToList();
                Assert.AreEqual(2, triplesWithId1Object.Count);
            foreach (var tripleWithId1Obj in triplesWithId1Object)
            {
                Assert.AreEqual(id1, tripleWithId1Obj.Object);
                Assert.AreEqual(inOrg, tripleWithId1Obj.Predicate);
            }
        }

        [ClassCleanup()]
        public static void TestEndingCleanup()
        {
            if (Directory.Exists(_dataDirectory))
                Directory.Delete(_dataDirectory, true);
        }
    }
}
// store.ClearAll();
            //store.Add(new TripleOV[]
            //{
            //    new TripleOV(
            //        new OV_iri("id1"),
            //        new OV_iri("a"),
            //        new OV_iri("country")),
            //    new TripleOV(
            //        new OV_iri("id2"),
            //        new OV_iri("a"),
            //        new OV_iri("person")),
            //    new TripleOV(
            //        new OV_iri("id3"),
            //        new OV_iri("a"),
            //        new OV_iri("person")),
            //    new TripleOV(
            //        new OV_iri("id4"),
            //        new OV_iri("a"),
            //        new OV_iri("participation")),
            //    new TripleOV(
            //        new OV_iri("id5"),
            //        new OV_iri("a"),
            //        new OV_iri("participation")),
            //    new TripleOV(
            //        new OV_iri("id1"),
            //        new OV_iri("name"),
            //        new OV_string("Россиия")),
            //    new TripleOV(
            //        new OV_iri("id2"),
            //        new OV_iri("name"),
            //        new OV_string("Владимир Владимирович")),
            //    new TripleOV(
            //        new OV_iri("id3"),
            //        new OV_iri("name"),
            //        new OV_string("Дмитрий Анатольевич")),
            //    new TripleOV(
            //        new OV_iri("id4"),
            //        new OV_iri("participant"),
            //        new OV_iri("id1")),
            //    new TripleOV(
            //        new OV_iri("id5"),
            //        new OV_iri("participant"),
            //        new OV_iri("id2")),
            //    new TripleOV(
            //        new OV_iri("id4"),
            //        new OV_iri("in-org"),
            //        new OV_iri("id1")),
            //    new TripleOV(
            //        new OV_iri("id5"),
            //        new OV_iri("in-org"),
            //        new OV_iri("id1"))
            //});