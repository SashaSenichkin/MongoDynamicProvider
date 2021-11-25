using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tests
{
    public class TestModel
    {
        [BsonElement("_id")]
        [JsonProperty("_id")]
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("otherFieldName")]
        public string StringField1 { get; set; }

        public string StringField2 { get; set; }

        [BsonElement("otherFieldName2")]
        public int IntField1 { get; set; }

        public int IntField2 { get; set; }

        [BsonElement("otherFieldName3")]
        public double DoubleField1 { get; set; }

        public double DoubleField2 { get; set; }

        public List<string> ArrayField { get; set; }
        
        public List<TestSubCLass> CustomArrayField { get; set; }
        
        public class TestSubCLass
        {
            public string StringField { get; set; }
        }
    }

}
