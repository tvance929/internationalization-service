using System;
using InternationalizationService.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;

namespace InternationalizationService.Config
{
    /// <summary>
    /// To add Serilog to the logging pipeline.
    /// </summary>
    public class SerilogConfig
    {
        /// <summary>
        /// To configure the Serilog using environment configurations and add serilog to the logging pipeline.
        /// </summary>
        /// <param name="serviceCollection">The collection of services.</param>
        /// <param name="configuration">The configuration properties.</param>
        public static void Configure(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var baseLogPath = configuration["Logging:Path"];
            if (string.IsNullOrEmpty(baseLogPath))
            {
                throw new ConfigurationErrorsException("Logging:Path is not set in the app.settings.");
            }

            var deploymentMode = configuration["DeploymentMode"];
            if (string.IsNullOrEmpty(deploymentMode))
            {
                throw new ConfigurationErrorsException("DeploymentMode is not set in the app.settings.");
            }

            var appName = configuration["ApplicationName"];
            if (string.IsNullOrEmpty(appName))
            {
                throw new ConfigurationErrorsException("ApplicationName is not set in the app.settings.");
            }

            var datadogAPIKey = configuration["DatadogAPIKey"];
            if (string.IsNullOrWhiteSpace(datadogAPIKey))
            {
                throw new ConfigurationErrorsException("DatadogAPIKey is not set in the web.config.");
            }

            var appLogName = $"{appName}-{deploymentMode.ToLower()}";
            var detailedLogFile = baseLogPath + "\\" + appLogName + $"-{DateTime.Now.ToString("yyyy-MM-dd")}.log";

            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] <" + deploymentMode + "|{SourceContext}|{CorrelationId}> {Message}{NewLine}{NewLine}{Exception}{NewLine}";

            switch (deploymentMode.ToUpper())
            {
                case "LOCAL":
                    // This will only write to the local file system as this is the dev's local machine.
                    SerilogLoggingLevelSwitch.SetLoggingLevel((int)LogEventLevel.Debug);
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithEnvironmentUserName()
                        .MinimumLevel.ControlledBy(SerilogLoggingLevelSwitch.LevelSwitch)
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Mvc"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Server.Kestrel"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.Matching.DfaMatcher"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.RouteValuesAddress"))
                        .WriteTo.File(detailedLogFile, SerilogLoggingLevelSwitch.LevelSwitch.MinimumLevel, outputTemplate, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 1024 * 1024 * 100) // 100MB
                        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                        .CreateLogger();
                    break;

                case "DEV":
                case "QA":
                    SerilogLoggingLevelSwitch.SetLoggingLevel((int)LogEventLevel.Warning);
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithEnvironmentUserName()
                        .MinimumLevel.ControlledBy(SerilogLoggingLevelSwitch.LevelSwitch)
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Mvc"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Server.Kestrel"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.Matching.DfaMatcher"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.RouteValuesAddress"))
                        .WriteTo.DatadogLogs(datadogAPIKey, "Serilog", appName, appName, new[] { $"environment:{deploymentMode}" })
                        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                        .CreateLogger();
                    break;

                case "STAGE":
                case "TEST":
                case "DEMO":
                case "TRAINING":
                case "PROD":
                    SerilogLoggingLevelSwitch.SetLoggingLevel((int)LogEventLevel.Warning);
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithEnvironmentUserName()
                        .MinimumLevel.ControlledBy(SerilogLoggingLevelSwitch.LevelSwitch)
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Mvc"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Server.Kestrel"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.Matching.DfaMatcher"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.RouteValuesAddress"))
                        .WriteTo.DatadogLogs(datadogAPIKey, "Serilog", appName, appName, new[] { $"environment:{deploymentMode}" })
                        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                        .CreateLogger();
                    break;

                default:
                    throw new IndexOutOfRangeException($"Unknown Deployment Mode found: {deploymentMode}");
            }

            serviceCollection.AddLogging(lb => lb.AddSerilog(dispose: true));

            var logger = Log.ForContext<SerilogConfig>();
            if (deploymentMode.ToUpper() == "LOCAL")
            {
                logger.Information($"Detailed log file will be written to {detailedLogFile}");
            }
            else
            {
                logger.Information($"Detailed log data is being sent to Datadog under the service name {appName}.");
            }

            logger.Debug("Startup -> Logging Configuration: COMPLETE");
        }
    }
}