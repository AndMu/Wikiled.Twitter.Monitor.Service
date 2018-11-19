using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class TrackingConfigFactory : ITrackingConfigFactory
    {
        private readonly ILogger<TrackingConfigFactory> logger;

        private readonly ITrackingManager manager;

        public TrackingConfigFactory(ILogger<TrackingConfigFactory> logger, TwitterConfig config, ITrackingManager manager)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public TwitterConfig Config { get; }

        public IKeywordTracker[] GetTrackers()
        {
            List<IKeywordTracker> tracker = new List<IKeywordTracker>();
            if (Config.Keywords?.Length > 0)
            {
                logger.LogDebug("Adding keywords");
                tracker.AddRange(Config.Keywords.Select(item => new KeywordTracker(item, true, manager.Resolve(item, "Keyword"))));
                logger.LogDebug("Total keywords: {0}", tracker.Count);
            }

            if (Config.Users?.Length > 0)
            {
                logger.LogDebug("Adding users");
                tracker.AddRange(Config.Users.Where(item => item.StartsWith("@")).Select(item => new KeywordTracker(item, false, manager.Resolve(item, "User"))));
                logger.LogDebug("Total keywords: {0}", tracker.Count);
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
