using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reflection;
using Wikiled.Common.Net.Client;
using Wikiled.Common.Utilities.Config;
using Wikiled.Sentiment.Api.Request;
using Wikiled.Sentiment.Api.Service;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Sentiment.Tracking.Modules;
using Wikiled.Server.Core.Errors;
using Wikiled.Server.Core.Helpers;
using Wikiled.Twitter.Modules;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;
using Wikiled.Twitter.Persistency;
using Wikiled.Twitter.Security;

namespace Wikiled.Twitter.Monitor.Service
{
    public class Startup
    {
        private readonly ILogger<Startup> logger;

        public Startup(ILoggerFactory loggerFactory, IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
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
            TwitterConfig twitter = services.RegisterConfiguration<TwitterConfig>(Configuration.GetSection("twitter"));
            SentimentConfig sentimentConfig = services.RegisterConfiguration<SentimentConfig>(Configuration.GetSection("sentiment"));

            services.AddMemoryCache();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the container builder.
            ContainerBuilder builder = new ContainerBuilder();
            SetupServices(builder, sentimentConfig);
            SetupOther(builder, sentimentConfig);
            SetupTracking(builder, twitter.Persistency);
            builder.Populate(services);
            IContainer appContainer = builder.Build();

            // start stream
            logger.LogInformation("Ready!");
            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(appContainer);
        }

        private void SetupTracking(ContainerBuilder builder, string path)
        {
            TrackingConfiguration config = new TrackingConfiguration(TimeSpan.FromHours(1), TimeSpan.FromDays(10), Path.Combine(path, "ratings.csv"))
            {
                Restore = true
            };

            builder.RegisterModule(new TrackingModule(config));
            builder.RegisterType<TrackingConfigFactory>().As<ITrackingConfigFactory>();
            builder.RegisterType<TrackingInstance>().As<ITrackingInstance>().SingleInstance();

            TimingStreamConfig trackingConfig = new TimingStreamConfig(path, TimeSpan.FromDays(1));
            builder.RegisterInstance(trackingConfig);
            builder.RegisterType<TimingStreamSource>().As<IStreamSource>();
        }

        private void SetupOther(ContainerBuilder builder, SentimentConfig sentiment)
        {
            builder.RegisterInstance(TaskPoolScheduler.Default).As<IScheduler>();
            builder.RegisterType<IpResolve>().As<IIpResolve>();
            builder.RegisterType<ApplicationConfiguration>().As<IApplicationConfiguration>();
            builder.RegisterModule<TwitterModule>();
            builder.RegisterType<TwitPersistency>().As<ITwitPersistency>();

            builder.RegisterType<EnvironmentAuthentication>().As<IAuthentication>();
            if (sentiment.Track)
            {
                builder.RegisterType<SentimentAnalysis>().As<ISentimentAnalysis>();
            }
            else
            {
                builder.RegisterType<NullSentimentAnalysis>().As<ISentimentAnalysis>();
            }

            builder.RegisterType<DublicateDetectors>().As<IDublicateDetectors>();
            builder.RegisterType<StreamMonitor>().AsSelf().As<IStreamMonitor>().SingleInstance().AutoActivate();
        }

        private void SetupServices(ContainerBuilder builder, SentimentConfig sentiment)
        {
            logger.LogInformation("Setting up services...");
            builder.Register(context => new StreamApiClientFactory(context.Resolve<ILoggerFactory>(),
                                                            new HttpClient { Timeout = TimeSpan.FromMinutes(10) },
                                                            new Uri(sentiment.Url)))
                .As<IStreamApiClientFactory>();
            WorkRequest request = new WorkRequest
            {
                CleanText = true,
                Domain = sentiment.Domain
            };
            builder.RegisterInstance(request);
            builder.RegisterType<SentimentAnalysis>().As<ISentimentAnalysis>();
            logger.LogInformation("Register sentiment: {0} {1}", sentiment.Url, sentiment.Domain);
        }
    }
}
