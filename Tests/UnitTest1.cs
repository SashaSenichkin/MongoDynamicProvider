using MongoDB.Bson;
using NUnit.Framework;
using Stp.Tools.MongoDB;
using System.IO;
using System;


namespace Tests
{
    public class Tests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestCorrectSet()
        {
            var json = File.ReadAllText("../../../Files/correctSet.json");
            var bson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            var entity = MongoDataProvider<TestModel>.Convert(bson);
            Assert.AreEqual(entity.StringField1, "test1");
            Assert.AreEqual(entity.StringField2, "test2");
            Assert.AreEqual(entity.IntField1, 1);
            Assert.AreEqual(entity.IntField2, 2);
            Assert.AreEqual(entity.DoubleField1, 1.11d);
            Assert.AreEqual(entity.DoubleField2, 2.22d);
            Assert.AreEqual(entity.ArrayField[0], "arr1");
            Assert.AreEqual(entity.CustomArrayField[0].StringField, "test1");

        }

        [Test]
        public void TestOverSet()
        {
            var json = File.ReadAllText("../../../Files/overSet.json");
            var bson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            var entity = MongoDataProvider<TestModel>.Convert(bson);
            Assert.AreEqual(entity.StringField1, "test1");
            Assert.AreEqual(entity.StringField2, "test2");
            Assert.AreEqual(entity.IntField1, 1);
            Assert.AreEqual(entity.IntField2, 2);
            Assert.AreEqual(entity.DoubleField1, 1.11d);
            Assert.AreEqual(entity.DoubleField2, 2.22d);
            Assert.AreEqual(entity.ArrayField[0], "arr1");
            Assert.AreEqual(entity.CustomArrayField[0].StringField, "test1");

        }

        [Test]
        public void TestLackSet()
        {
            var json = File.ReadAllText("../../../Files/lackSet.json");
            var bson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            var entity = MongoDataProvider<TestModel>.Convert(bson);
            Assert.AreEqual(entity.StringField1, "test1");
            Assert.AreEqual(entity.IntField1, 1);
            Assert.AreEqual(entity.IntField2, 2);
            Assert.AreEqual(entity.DoubleField2, 2.22d);
            Assert.AreEqual(entity.ArrayField[0], "arr1");
            Assert.AreEqual(entity.CustomArrayField[0].StringField, "test1");

        }
    }
}