using System;
using System.Collections.Generic;
using System.Linq;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class TrackingConfigFactory : ITrackingConfigFactory
    {
        private readonly TwitterConfig config;

        private readonly IApplicationConfiguration application;

        public TrackingConfigFactory(TwitterConfig config, IApplicationConfiguration application)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.application = application ?? throw new ArgumentNullException(nameof(application));
        }

        public string GetPath()
        {
            return config.Persistency;
        }

        public ITracker[] GetTrackers()
        {
            List<ITracker> tracker = new List<ITracker>();
            if (config.Keywords?.Length > 0)
            {
                tracker.AddRange(config.Keywords.Select(item => new Tracker(application, item, true)));
            }

            if (config.Users?.Length > 0)
            {
                tracker.AddRange(config.Users.Where(item => item.StartsWith("@")).Select(item => new Tracker(application, item, false)));
            }

            return tracker.ToArray();
        }

        public LanguageFilter[] GetLanguages()
        {
            if (config.Languages?.Any() != true)
            {
                return null;
            }

            return config.Languages.Select(
                             item =>
                             {
                                 if (string.IsNullOrWhiteSpace(item))
                                 {
                                     return (LanguageFilter?)null;
                                 }

                                 if (Enum.TryParse(item, out LanguageFilter value))
                                 {
                                     return value;
                                 }

                                 throw new Exception("Unknown language: " + item);
                             })
                         .Where(item => item != null)
                         .Select(item => item.Value)
                         .ToArray();
        }
    }
}
