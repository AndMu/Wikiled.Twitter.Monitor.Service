using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;
using Wikiled.Common.Extensions;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Sentiment.Api.Service;
using Wikiled.Twitter.Persistency;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class TrackingInstance : ITrackingInstance
    {
        private readonly TimingStreamSource streamSource;

        private readonly ISentimentAnalysis sentiment;

        private readonly TwitPersistency persistency;

        private readonly Dictionary<string, IKeywordTracker> keywordTrackers = new Dictionary<string, IKeywordTracker>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, IKeywordTracker> userTrackers = new Dictionary<string, IKeywordTracker>(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<TrackingInstance> logger;

        public TrackingInstance(ITrackingConfigFactory trackingConfigFactory, ISentimentAnalysis sentiment, ILoggerFactory loggerFactory)
        {
            if (trackingConfigFactory == null)
            {
                throw new ArgumentNullException(nameof(trackingConfigFactory));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            string path = trackingConfigFactory.Config.Persistency;
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Value cannot be null or whitespace", nameof(path));
            }

            logger = loggerFactory.CreateLogger<TrackingInstance>();
            this.sentiment = sentiment ?? throw new ArgumentNullException(nameof(sentiment));
            Trackers = trackingConfigFactory.GetTrackers();
            Languages = trackingConfigFactory.GetLanguages();
            path.EnsureDirectoryExistence();
            streamSource = new TimingStreamSource(path, TimeSpan.FromDays(1));
            persistency = new TwitPersistency(streamSource);

            foreach (IKeywordTracker tracker in Trackers)
            {
                if (tracker.IsKeyword)
                {
                    keywordTrackers[tracker.Keyword] = tracker;
                    if (trackingConfigFactory.Config.HashKeywords &&
                        !tracker.Keyword.StartsWith("#"))
                    {
                        keywordTrackers["#" + tracker.RawKeyword] = tracker;
                    }

                }
                else
                {
                    userTrackers[tracker.Keyword] = tracker;
                }
            }
        }

        public IKeywordTracker[] Trackers { get; }

        public LanguageFilter[] Languages { get; }

        public async Task OnReceived(ITweetDTO tweet)
        {
            try
            {
                double? sentimentValue = await sentiment.Measure(tweet.Text).ConfigureAwait(false);
                ITweet tweetItem = Tweet.GenerateTweetFromDTO(tweet);
                Task saveTask = Task.Run(() => persistency?.Save(tweetItem, sentimentValue));
                RatingRecord rating = new RatingRecord(DateTime.UtcNow, sentimentValue);
                foreach (IKeywordTracker tracker in Trackers)
                {
                    tracker.Tracker.AddRating(rating);
                }

                if (userTrackers.TryGetValue(tweet.CreatedBy.Name, out IKeywordTracker trackerUser))
                {
                    trackerUser.Tracker.AddRating(rating);
                }

                await saveTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing");
            }
        }

        public IKeywordTracker Resolve(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (keywordTrackers.TryGetValue(key, out IKeywordTracker value))
            {
                return value;
            }

            userTrackers.TryGetValue(key, out value);
            return value;
        }

        public void Dispose()
        {
            streamSource.Dispose();
        }
    }
}
