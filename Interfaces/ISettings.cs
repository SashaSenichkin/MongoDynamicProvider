namespace Stp.Tools.MongoDB.Interfaces
{
    /// <summary>
    /// File settings MongoDb
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Connection string to MongoDb
        /// </summary>
        string ConnectionString { get; set; }
    }
}