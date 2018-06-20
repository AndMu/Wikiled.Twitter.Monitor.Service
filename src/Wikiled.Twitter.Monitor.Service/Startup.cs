using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using Wikiled.Server.Core.Helpers;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Wikiled.Twitter.Monitor.Service
{
    public class Startup
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Env = env;
            ReconfigureLogging();
            logger.Info($"Starting: {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        public IConfigurationRoot Configuration { get; }

        public IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCors(
                options =>
                {
                    options.AddPolicy(
                        "CorsPolicy",
                        itemBuider => itemBuider.AllowAnyOrigin()
                                                .AllowAnyMethod()
                                                .AllowAnyHeader()
                                                .AllowCredentials());
                });

            // Add framework services.
            services.AddMvc(options => { });

            // needed to load configuration from appsettings.json
            services.AddOptions();

            services.Configure<TwitterConfig>(Configuration.GetSection("twitter"));
            services.Configure<SentimentConfig>(Configuration.GetSection("sentiment"));

            services.AddMemoryCache();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the container builder.
            var builder = new ContainerBuilder();
            SetupOther(builder);
            builder.Populate(services);
            var appContainer = builder.Build();
            logger.Info("Ready!");
            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(appContainer);
        }

        private void SetupOther(ContainerBuilder builder)
        {
            builder.RegisterType<IpResolve>().As<IIpResolve>();
            builder.RegisterType<TrackingConfigFactory>().As<ITrackingConfigFactory>();
            builder.RegisterType<TrackingInstance>().As<ITrackingInstance>();
            builder.RegisterType<StreamApiClientFactory>().As<IStreamApiClientFactory>();
            builder.RegisterType<SentimentAnalysis>().As<ISentimentAnalysis>();
            builder.RegisterType<DublicateDetectors>().As<IDublicateDetectors>();
            builder.RegisterType<StreamMonitor>().As<IStreamMonitor>().SingleInstance();
        }

        private void ReconfigureLogging()
        {
            // manually refresh of NLog configuration
            // as it is not picking up global
            LogManager.Configuration.Variables["logDirectory"] =
                Configuration.GetSection("logging").GetValue<string>("path");

            var logLevel = Configuration.GetValue<LogLevel>("Logging:LogLevel:Default");
            switch (logLevel)
            {
                case LogLevel.Trace:
                    LogManager.GlobalThreshold = NLog.LogLevel.Trace;
                    break;
                case LogLevel.Debug:
                    LogManager.GlobalThreshold = NLog.LogLevel.Debug;
                    break;
                case LogLevel.Information:
                    LogManager.GlobalThreshold = NLog.LogLevel.Info;
                    break;
                case LogLevel.Warning:
                    LogManager.GlobalThreshold = NLog.LogLevel.Warn;
                    break;
                case LogLevel.Error:
                    LogManager.GlobalThreshold = NLog.LogLevel.Error;
                    break;
                case LogLevel.Critical:
                    LogManager.GlobalThreshold = NLog.LogLevel.Fatal;
                    break;
                case LogLevel.None:
                    LogManager.GlobalThreshold = NLog.LogLevel.Off;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
