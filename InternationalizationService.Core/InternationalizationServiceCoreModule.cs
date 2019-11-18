using System.Reflection;
using Autofac;
using InternationalizationService.Core.Database;
using InternationalizationService.Core.Database.Interface;
using InternationalizationService.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using NPoco;
using Serilog;
using VPT.Caching.Utility;
using VPT.Caching.Utility.Interface;
using Module = Autofac.Module;

namespace InternationalizationService.Core
{
    /// <summary>
    /// To register components of Rules Service.
    /// </summary>
    public class RulesServiceCoreModule : Module
    {
        private static readonly ILogger Logger = Log.ForContext<RulesServiceCoreModule>();

        private readonly IConfiguration Configuration;

        /// <summary>
        /// The constructor of <see cref="RulesServiceCoreModule"/> type.
        /// </summary>
        /// <param name="configuration"></param>
        public RulesServiceCoreModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            var RulesConnectionString = Configuration.GetConnectionString("InternationalizationServiceConnectionString");
            if (string.IsNullOrEmpty(RulesConnectionString))
            {
                throw new ConfigurationErrorsException("Internationalization Service connection string is not set in appsettings.json.");
            }

            builder.Register(c => new RulesServiceDatabase(RulesConnectionString, DatabaseType.SqlServer2012, System.Data.SqlClient.SqlClientFactory.Instance))
               .As<IInternationalizationServiceDatabase>()
               .InstancePerDependency();

            //register repositories
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces();

            //register the services
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces();

            builder.RegisterType<RedisCacheUtilityV2>().As<ICacheUtility>();
            builder.RegisterType<ConfigurationCacheUtilityV2>().As<IConfigurationCacheUtility>();

            Logger.Debug("Startup -> AutoFac DeveloperPortalCoreModule Module Registration: COMPLETE");
        }
    }
}
