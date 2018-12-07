using Autofac;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Wikiled.Sentiment.Tracking.Service;
using Wikiled.Server.Core.Helpers;
using Wikiled.Twitter.Modules;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;
using Wikiled.Twitter.Persistency;
using Wikiled.Twitter.Security;

namespace Wikiled.Twitter.Monitor.Service
{
    public class Startup : BaseStartup
    {
        private readonly ILogger<Startup> logger;

        private TwitterConfig config;

        public Startup(ILoggerFactory loggerFactory, IHostingEnvironment env)
            : base(loggerFactory, env)
        {
            logger = loggerFactory.CreateLogger<Startup>();
        }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            config = services.RegisterConfiguration<TwitterConfig>(Configuration.GetSection("twitter"));
            return base.ConfigureServices(services);
        }

        private void SetupTracking(ContainerBuilder builder)
        {
            builder.RegisterType<TrackingConfigFactory>().As<ITrackingConfigFactory>();
            builder.RegisterType<TrackingInstance>().As<ITrackingInstance>().SingleInstance();
            builder.RegisterType<TimingStreamSource>().As<IStreamSource>();
        }

        private void SetupOther(ContainerBuilder builder)
        {
            builder.RegisterModule<TwitterModule>();
            builder.RegisterType<TwitPersistency>().As<ITwitPersistency>();
            builder.RegisterType<EnvironmentAuthentication>().As<IAuthentication>();
            builder.RegisterType<DublicateDetectors>().As<IDublicateDetectors>();
            builder.RegisterType<StreamMonitor>().AsSelf().As<IStreamMonitor>().SingleInstance().AutoActivate();
            TimingStreamConfig trackingConfig = new TimingStreamConfig(GetPersistencyLocation(), TimeSpan.FromDays(1));
            builder.RegisterInstance(trackingConfig);
        }


        protected override void ConfigureSpecific(ContainerBuilder builder)
        {
            SetupTracking(builder);
            SetupOther(builder);
        }

        protected override string GetPersistencyLocation()
        {
            return config.Persistency;
        }
    }
}
