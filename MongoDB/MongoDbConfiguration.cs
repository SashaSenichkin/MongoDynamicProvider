using Microsoft.Extensions.DependencyInjection;
using Stp.Tools.MongoDB.Interfaces;

namespace Stp.Tools.MongoDB
{
    /// <summary>
    /// MongoDb extension
    /// </summary>
    public static class MongoDbConfiguration
    {
        /// <summary>   
        /// Adding and configuring MongoDb
        /// </summary>
        /// <param name="services"></param>
        public static void AddMongoDb(this IServiceCollection services)
        {
            services.AddSingleton<IMongoDbSettings>(sp =>
                sp.GetRequiredService<MongoDbSettings>());
        }
    }
}
