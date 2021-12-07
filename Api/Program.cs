using Common;
using Ecosystem.ML.LanguageProcessingApi.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;

namespace Ecosystem.ML.LanguageProcessingApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = LoggingConfigurations.GetLogger();

            try
            {
                Log.Information("Application starting up.");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start up correctly.");
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            //CreateDefaultBuilder calls AddUserSecrets when the EnvironmentName is Development. Secrets will be access through IConfiguration[Movies:ServiceApiKey]
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    var builtConfig = config.SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", false)
                                    .AddJsonFile($"appsettings.Production.json", true)
                                    .AddEnvironmentVariables()
                                    .Build();
                    var settings = builtConfig.Get<AppSettings>();

                    if (settings.ContainerMode)
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            //1: Add plain secrets (no configs) as Opaque secrets manually
                            //2: Secrets are accessible through separate files
                            //3: Read the secret map values through container volumes

                            config.AddKeyPerFile($"/root/.microsoft/usersecrets");
                        }

                        if (context.HostingEnvironment.IsProduction())
                        {
                            //1: Add plain secrets (no configs) in KeyVault
                            //2: Secrets are accessible through separate files
                            //3: Read the secret map values through CSI KeyVault Drivers for AKS
                            config.AddKeyPerFile($"/root/.microsoft/usersecrets");
                        }
                    }
                });
    }
}