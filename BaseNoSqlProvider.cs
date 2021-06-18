using Stp.Tools.MongoDB.Interfaces;

namespace Stp.Tools.MongoDB
{
    /// <summary>
    /// Base provider for NoSQL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseNoSqlProvider<T> where T : class
    {
        private readonly IMongoDbSettings _settings;
        private readonly string _nameCollection;

        protected BaseNoSqlProvider(IMongoDbSettings settings, string nameCollection)
        {
            _settings = settings;
            _nameCollection = nameCollection;
        }

        /// <summary>
        /// Get context by connection string
        /// </summary>
        /// <returns></returns>
        protected MongoContext<T> GetContext()
        {
            return new(_settings, _nameCollection);
        }
    }
}
