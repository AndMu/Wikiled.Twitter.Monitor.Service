using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class TrackingConfigFactory : ITrackingConfigFactory
    {
        private readonly IOptions<TwitterConfig> config;

        private readonly IApplicationConfiguration application;

        public TrackingConfigFactory(IOptions<TwitterConfig> config, IApplicationConfiguration application)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.application = application ?? throw new ArgumentNullException(nameof(application));
        }

        public string GetPath()
        {
            return config.Value.Persistency;
        }

        public IKeywordTracker[] GetTrackers()
        {
            return config.Value.Keywords.Select(item => new KeywordTracker(application, item)).ToArray();
        }

        public LanguageFilter[] GetLanguages()
        {
            throw new NotImplementedException();
        }
    }
}
