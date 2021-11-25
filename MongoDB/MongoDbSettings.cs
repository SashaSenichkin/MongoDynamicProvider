using Stp.Tools.MongoDB.Interfaces;

namespace Stp.Tools.MongoDB
{
    public class MongoDbSettings : IMongoDbSettings
    {
        public string ConnectionString { get; set; }
        
        public string DatabaseName { get; set; }
    }
}
