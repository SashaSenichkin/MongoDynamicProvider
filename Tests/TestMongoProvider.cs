using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Stp.Tools.MongoDB;
using Stp.Tools.MongoDB.Interfaces;

namespace Tests
{
    public class TestMongoProvider : MongoDataProvider<TestModel>
    {
        public TestMongoProvider(IMongoDbSettings settings, string nameCollection) : base(settings, nameCollection)
        {
        }

        public static TestModel Convert(BsonDocument source)
        {
            var list = new List<BsonDocument> {source};
            return MongoDataProvider<TestModel>.Convert<TestModel>(list).Item1.First();
        }
    }
}