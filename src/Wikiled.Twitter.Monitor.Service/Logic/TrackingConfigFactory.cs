﻿using System;
using System.Linq;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic
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

        public IKeywordTracker[] GetTrackers()
        {
            return config.Keywords.Select(item => new KeywordTracker(application, item)).ToArray();
        }

        public LanguageFilter[] GetLanguages()
        {
            throw new NotImplementedException();
        }
    }
}