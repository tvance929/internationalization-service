using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InternationalizationService.Core.Config;
using VPT.Caching;

namespace InternationalizationService.Config
{
    /// <summary>
    /// The extension methods for <see cref="IServiceCollection"/> type.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the configurations using Options pattern.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> type.</param>
        /// <param name="configuration">The configuration properties.</param>
        /// <returns></returns>
        public static IServiceCollection RegisterConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BaseURLOptions>(configuration.GetSection("BaseURLs"));
            services.Configure<ServiceToServiceAuthOptions>(configuration.GetSection("ServiceToServiceBasicAuth"));

            //Register base URL so cache utility can refill the organization configurations.
            services.Configure<ConfigurationCacheOptions>(options =>
            {
                options.AccountsAPIBaseUrl = configuration["BaseURLs:AccountsAPIBaseURL"];
            });

            return services;
        }
    }
}
