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
using Wikiled.Common.Utilities.Config;
using Wikiled.Server.Core.Errors;
using Wikiled.Server.Core.Helpers;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic;
using Wikiled.Twitter.Monitor.Service.Logic.Sentiment;
using Wikiled.Twitter.Security;

namespace Wikiled.Twitter.Monitor.Service
{
    public class Startup
    {
        private readonly ILogger<Startup> logger;

        public Startup(ILoggerFactory loggerFactory, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Env = env;
            logger = loggerFactory.CreateLogger<Startup>();
            Configuration.ChangeNlog();
            logger.LogInformation($"Starting: {Assembly.GetExecutingAssembly().GetName().Version}");
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
            app.UseExceptionHandlingMiddleware();
            app.UseHttpStatusCodeExceptionMiddleware();

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
            services.RegisterConfiguration<TwitterConfig>(Configuration.GetSection("twitter"));
            var sentimentConfig = services.RegisterConfiguration<SentimentConfig>(Configuration.GetSection("sentiment"));

            services.AddMemoryCache();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the container builder.
            var builder = new ContainerBuilder();
            SetupOther(builder, sentimentConfig);
            builder.Populate(services);
            var appContainer = builder.Build();
            // start stream
            appContainer.Resolve<IStreamMonitor>();
            logger.LogInformation("Ready!");
            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(appContainer);
        }

        private void SetupOther(ContainerBuilder builder, SentimentConfig sentiment)
        {
            builder.RegisterType<IpResolve>().As<IIpResolve>();
            builder.RegisterType<ApplicationConfiguration>().As<IApplicationConfiguration>();
            builder.RegisterType<EnvironmentAuthentication>().As<IAuthentication>();
            builder.RegisterType<TrackingConfigFactory>().As<ITrackingConfigFactory>();
            builder.RegisterType<TrackingInstance>().As<ITrackingInstance>();
            builder.RegisterType<StreamApiClientFactory>().As<IStreamApiClientFactory>();
            if (sentiment.Track)
            {
                builder.RegisterType<SentimentAnalysis>().As<ISentimentAnalysis>();
            }
            else
            {
                builder.RegisterType<NullSentimentAnalysis>().As<ISentimentAnalysis>();
            }

            builder.RegisterType<DublicateDetectors>().As<IDublicateDetectors>();
            builder.RegisterType<StreamMonitor>().As<IStreamMonitor>().SingleInstance();
        }
    }
}
