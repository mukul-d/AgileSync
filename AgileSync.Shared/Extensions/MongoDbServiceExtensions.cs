using AgileSync.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AgileSync.Shared.Extensions;

/// <summary>
/// Extension members for registering MongoDB services into the DI container.
/// Uses the C# 14 extension block syntax.
/// </summary>
public static class MongoDbServiceExtensions
{
    /// <param name="services">The service collection to configure.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers a MongoDB client and database as singletons using settings from configuration.
        /// </summary>
        /// <param name="configuration">The application configuration containing the "MongoDb" section.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when MongoDb settings are missing.</exception>
        public IServiceCollection AddMongoDb(IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

            var settings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>()
                ?? throw new InvalidOperationException("MongoDb settings are not configured.");

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            services.AddSingleton<IMongoClient>(client);
            services.AddSingleton(database);

            return services;
        }
    }
}
