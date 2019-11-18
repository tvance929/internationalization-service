using Microsoft.Extensions.Configuration;
using InternationalizationService.Core.Exceptions;
using VPT.Caching.Config;

namespace InternationalizationService.Config
{
    /// <summary>
    /// Configure the Redis cache connection
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// Configure Redis
        /// </summary>
        /// <param name="configuration">Configuration</param>
        public static void Configure(IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("RedisConnectionString");
            if (string.IsNullOrEmpty(redisConnection))
            {
                throw new ConfigurationErrorsException("Redis connection string not found in appsettings.json");
            }
            RedisConnectionMultiplexer.InitializeConnectionString(redisConnection);
        }
    }
}
