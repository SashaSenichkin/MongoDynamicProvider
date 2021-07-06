using MongoDB.Driver;
using Stp.Tools.MongoDB.Interfaces;

namespace Stp.Tools.MongoDB
{
    /// <summary>
    /// MongoClient for work with MongoDb
    /// </summary>
    public class MongoContext<T> : MongoClient where T : class
    {
        private readonly IMongoDatabase _db;
        private readonly string _collectionName;

        public MongoContext(IMongoDbSettings settings, string collectionName) : base(settings.ConnectionString)
        {
            _collectionName = collectionName;
            _db = GetDatabase(settings.DatabaseName);
        }
        
        /// <summary>
        /// Represents a typed collection in MongoDB queryable source
        /// </summary>
        public IMongoCollection<T> Source =>
            _db.GetCollection<T>(_collectionName);
    }
}