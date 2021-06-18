using MongoDB.Driver;
using Stp.Tools.MongoDB.Interfaces;

namespace Stp.Tools.MongoDB
{
    /// <summary>
    /// MongoClient for work with MongoDb
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoContext<T> : MongoClient where T : class
    {
        private readonly IMongoDatabase _db;
        private readonly string _nameCollection;

        public MongoContext(ISettings settings, string nameCollection) : base(settings.ConnectionString)
        {
            _nameCollection = nameCollection;
            _db = GetDatabase(Settings.Credential.Source);
        }

        /// <summary>
        ///     Represents a typed collection in MongoDB queryable source
        /// </summary>
        public IMongoCollection<T> Source => _db.GetCollection<T>(_nameCollection);
    }
}