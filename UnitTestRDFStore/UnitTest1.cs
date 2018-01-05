using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFCommon;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using RDFStore;

namespace UnitTestRDFStore
{
    [TestClass]
    public class UnitTestStore
    {
        [TestMethod]
        public void TestSimpleTable()
        {
            var turtleFilePath = @"..\..\..\Examples\simplest.ttl"; //Config.Source_data_folder_path + "1M.ttl";
            var dataDirectory = @"..\..\..\Database\";
            var store = new Store(dataDirectory);
             store.ReloadFrom(turtleFilePath);
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
            store.BuildIndexes();
            Assert.AreEqual(12, store.GetTriplesCount());
            Assert.IsTrue(store.Any());
            ObjectVariants id1;
            Assert.IsTrue(store.NodeGenerator.TryGetUri(new OV_iri("id1"), out  id1));
            ObjectVariants name;
            Assert.IsTrue(store.NodeGenerator.TryGetUri(new OV_iri("name"), out  name));
            var triplesWithSubjectPredicate = store.GetTriplesWithSubjectPredicate(id1, name);
            Assert.AreEqual(1, triplesWithSubjectPredicate.Count());
            Assert.AreEqual("Ðîññèÿ", triplesWithSubjectPredicate.First().Content);
        }
    }
}
