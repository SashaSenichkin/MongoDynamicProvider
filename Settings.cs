using Stp.Tools.MongoDB.Interfaces;

namespace Stp.Tools.MongoDB
{
    /// <summary>
    ///     File settings MongoDb
    /// </summary>
    public class Settings : ISettings
    {
        public string ConnectionString { get; set; }
    }
}