using AutoMapper;
using Business.Common;
using Business.Services;
using Business.Services.Abstract;
using Common;
using Common.Attributes;
using Common.Middleware;
using Ecosystem.ML.LanguageProcessingApi.Configuration;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ML;
using ML.DTO;
using ML.Prediction;
using ML.Prediction.Abstract;
using Serilog;
using System;
using System.IO;

namespace Ecosystem.ML.LanguageProcessingApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment CurrentEnvironment { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AppSettings appSettings = SetupConfiguration(services);

            services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy",
                builder =>
                {
                    builder.WithOrigins(
                            appSettings.
                            AllowedOrigins.
                            Split(","))
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            SetupAutoMapper(services);

            SetupDI(services);

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddHealthChecks()
                .AddCheck<ApiHealthCheck>("Check API Health");

            services.AddResponseCaching();

            ConfigureVersioning(services);

            ConfigureFluentValidation(services);
        }

        private static void ConfigureFluentValidation(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddApiExplorer();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ValidateModelStateAttribute));
            })
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());
        }

        private void ConfigureVersioning(IServiceCollection services)
        {
            services.AddControllersWithViews(o =>
            {
                o.UseGeneralRoutePrefix("/v{version:apiVersion}");
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = 1;
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore;
            });

            services.AddApiVersioning(o => o.ReportApiVersions = true);

            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";
                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });
        }

        private AppSettings SetupConfiguration(IServiceCollection services)
        {
            var environmentName = Environment.GetEnvironmentVariable(LanguageProcessingConstants.EnvironmentVariable);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var appSettings = configuration.Get<AppSettings>();
            services.AddSingleton(appSettings);
            return appSettings;
        }

        private void SetupDI(IServiceCollection services)
        {
            //Functions
            services.AddTransient<ILanguageProcessingService, LanguageProcessingService>();
            services.AddSingleton<MLContext>();
            services.AddSingleton<IModelTrainer, ModelTrainer>();
            services.AddSingleton<OnnxModelConfigurator<BertFeature>>();
            services.AddTransient<IBertModel, BertModel>();
            services.AddTransient<ITokeniser, WordTokeniser>();

            services.AddHttpClient("LanguageProcessingApi", c =>
            {
                // Github API versioning
                c.DefaultRequestHeaders.Add("User-Agent", "Ecosystem-ML-LanguageProcessingApi");
                c.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                c.Timeout = TimeSpan.FromMinutes(1);
            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }

        private void SetupAutoMapper(IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperConfiguration());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            CurrentEnvironment = env;

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
            }

            //Below order matters
            app.UseRouting();

            app.UseCors("CORSPolicy");

            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(10)
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" };

                await next();
            });

            app.UseSerilogRequestLogging();

            app.ConfigureCustomExceptionMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}