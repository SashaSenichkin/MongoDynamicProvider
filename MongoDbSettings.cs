using Stp.Tools.MongoDB.Interfaces;

namespace Stp.Tools.MongoDB
{
    /// <summary>
    ///     File settings MongoDb
    /// </summary>
    public class MongoDbSettings : IMongoDbSettings
    {
        public string ConnectionString { get; set; }
    }
}