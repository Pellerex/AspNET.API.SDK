using Business.Common;
using Common;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace Ecosystem.ML.LanguageProcessingApi.Configuration
{
    public class LoggingConfigurations
    {
        public static ILogger GetLogger()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            var telemetryConfiguration = new TelemetryConfiguration(configuration["Monitoring:AzureApplicationInsightsInstrumentationKey"]);

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration);

            if (Common.General.RequestEnvironment == "Production")
            {
                loggerConfiguration.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            return Log.Logger;
        }
    }
}