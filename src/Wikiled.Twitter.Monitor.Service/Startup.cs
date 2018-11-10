using System;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Client;
using Wikiled.Common.Utilities.Config;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Sentiment.Api.Request;
using Wikiled.Sentiment.Api.Service;
using Wikiled.Server.Core.Errors;
using Wikiled.Server.Core.Helpers;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;
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
            SetupServices(builder, sentimentConfig);
            SetupOther(builder, sentimentConfig);
            SetupTracking(builder);
            builder.Populate(services);
            var appContainer = builder.Build();
            // start stream
            appContainer.Resolve<IStreamMonitor>();
            logger.LogInformation("Ready!");
            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(appContainer);
        }

        private void SetupTracking(ContainerBuilder builder)
        {
            builder.RegisterType<TrackingConfigFactory>().As<ITrackingConfigFactory>();
            builder.RegisterType<TrackingInstance>().As<ITrackingInstance>();
            builder.RegisterInstance(new TrackingConfiguration(TimeSpan.FromHours(1), TimeSpan.FromDays(1)));
        }

        private void SetupOther(ContainerBuilder builder, SentimentConfig sentiment)
        {
            builder.RegisterInstance(TaskPoolScheduler.Default).As<IScheduler>();
            builder.RegisterType<IpResolve>().As<IIpResolve>();
            builder.RegisterType<ApplicationConfiguration>().As<IApplicationConfiguration>();
            builder.RegisterType<EnvironmentAuthentication>().As<IAuthentication>();
            builder.RegisterType<ExpireTracking>().As<IExpireTracking>().SingleInstance();
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

        private void SetupServices(ContainerBuilder builder, SentimentConfig sentiment)
        {
            logger.LogInformation("Setting up services...");
            builder.Register(context => new StreamApiClientFactory(context.Resolve<ILoggerFactory>(),
                                                            new HttpClient { Timeout = TimeSpan.FromMinutes(10) },
                                                            new Uri(sentiment.Url)))
                .As<IStreamApiClientFactory>();
            var request = new WorkRequest();
            request.CleanText = true;
            request.Domain = sentiment.Domain;
            builder.RegisterInstance(request);
            builder.RegisterType<SentimentAnalysis>().As<ISentimentAnalysis>(); 
            logger.LogInformation("Register sentiment: {0} {1}", sentiment.Url, sentiment.Domain);
        }
    }
}
