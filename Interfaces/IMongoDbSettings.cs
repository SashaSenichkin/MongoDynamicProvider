namespace Stp.Tools.MongoDB.Interfaces
{
    /// <summary>
    /// MongoDB settings as they are in the Vault's secret
    /// </summary>
    public interface IMongoDbSettings
    {
        /// <summary>
        /// <para>MongoDB connection string WITHOUT database name specified</para>
        /// <para>Format: mongodb://[username]:[userpassword]@[host]:[port]</para>
        /// </summary>
        public string ConnectionString { get; set; }
        
        /// <summary>
        /// MongoDB database name
        /// </summary>
        public string DatabaseName { get; set; }
    }
}