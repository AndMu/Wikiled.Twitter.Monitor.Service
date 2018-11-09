using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class TrackingConfigFactory : ITrackingConfigFactory
    {

        private readonly IApplicationConfiguration application;

        private readonly ILogger<TrackingConfigFactory> logger;

        private readonly IExpireTracking expireTracking;

        public TrackingConfigFactory(ILogger<TrackingConfigFactory> logger, TwitterConfig config, IApplicationConfiguration application, IExpireTracking expireTracking)
        {
            this.Config = config ?? throw new ArgumentNullException(nameof(config));
            this.application = application ?? throw new ArgumentNullException(nameof(application));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.expireTracking = expireTracking ?? throw new ArgumentNullException(nameof(expireTracking));
        }

        public TwitterConfig Config { get; }

        public IKeywordTracker[] GetTrackers()
        {
            List<IKeywordTracker> tracker = new List<IKeywordTracker>();
            if (Config.Keywords?.Length > 0)
            {
                logger.LogDebug("Adding keywords");
                tracker.AddRange(Config.Keywords.Select(item => new KeywordTracker(application, item, true)));
                logger.LogDebug("Total keywords: {0}", tracker.Count);
            }

            if (Config.Users?.Length > 0)
            {
                logger.LogDebug("Adding users");
                tracker.AddRange(Config.Users.Where(item => item.StartsWith("@")).Select(item => new KeywordTracker(application, item, false)));
                logger.LogDebug("Total keywords: {0}", tracker.Count);
            }

            foreach (var keywordTracker in tracker)
            {
                expireTracking.Register(keywordTracker.Tracker);
            }

            return tracker.ToArray();
        }

        public LanguageFilter[] GetLanguages()
        {
            if (Config.Languages?.Any() != true)
            {
                return null;
            }

            logger.LogDebug("Selecting languages");
            return Config.Languages.Select(
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
