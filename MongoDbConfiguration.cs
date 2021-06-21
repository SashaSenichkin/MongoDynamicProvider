using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        /// <param name="configuration"></param>
        public static void AddMongoDb(this IServiceCollection services)
        {
            services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
        }
    }
}
