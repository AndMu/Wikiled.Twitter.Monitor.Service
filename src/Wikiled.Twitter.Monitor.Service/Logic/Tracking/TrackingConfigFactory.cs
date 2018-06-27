﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class TrackingConfigFactory : ITrackingConfigFactory
    {
        private readonly TwitterConfig config;

        private readonly IApplicationConfiguration application;

        private readonly ILogger<TrackingConfigFactory> logger;

        public TrackingConfigFactory(TwitterConfig config, IApplicationConfiguration application, ILogger<TrackingConfigFactory> logger)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.application = application ?? throw new ArgumentNullException(nameof(application));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                logger.LogDebug("Adding keywords");
                tracker.AddRange(config.Keywords.Select(item => new Tracker(application, item, true)));
                logger.LogDebug("Total keywords: {0}", tracker.Count);
                if (config.HashKeywords)
                {
                    logger.LogDebug("Creating hashkey versions");
                    tracker.AddRange(config.Keywords.Where(item => !item.StartsWith("#")).Select(item => new Tracker(application, "#" + item, true)));
                    logger.LogDebug("Total keywords: {0}", tracker.Count);
                }
            }

            if (config.Users?.Length > 0)
            {
                logger.LogDebug("Adding users");
                tracker.AddRange(config.Users.Where(item => item.StartsWith("@")).Select(item => new Tracker(application, item, false)));
                logger.LogDebug("Total keywords: {0}", tracker.Count);
            }

            return tracker.ToArray();
        }

        public LanguageFilter[] GetLanguages()
        {
            if (config.Languages?.Any() != true)
            {
                return null;
            }

            logger.LogDebug("Selecting languages");
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
