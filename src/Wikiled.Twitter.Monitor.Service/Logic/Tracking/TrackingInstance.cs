using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        private readonly Dictionary<string, IKeywordTracker> keywordTrackers;

        private readonly Dictionary<string, IKeywordTracker> userTrackers;

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

            var path = trackingConfigFactory.GetPath();
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
            keywordTrackers = Trackers.Where(item => item.IsKeyword).ToDictionary(item => item.Value, item => item, StringComparer.OrdinalIgnoreCase);
            userTrackers = Trackers.Where(item => !item.IsKeyword).ToDictionary(item => item.Value, item => item, StringComparer.OrdinalIgnoreCase);
        }

        public IKeywordTracker[] Trackers { get; }

        public LanguageFilter[] Languages { get; }

        public async Task OnReceived(ITweetDTO tweet)
        {
            try
            {
                var sentimentValue = await sentiment.Measure(tweet.Text).ConfigureAwait(false);
                var tweetItem = Tweet.GenerateTweetFromDTO(tweet);
                var saveTask = Task.Run(() => persistency?.Save(tweetItem, sentimentValue));
                var rating = new RatingRecord(DateTime.UtcNow, sentimentValue);
                foreach (var tracker in Trackers)
                {
                    tracker.Tracker.AddRating(rating);
                }

                if (userTrackers.TryGetValue(tweet.CreatedBy.Name, out var trackerUser))
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

            if (keywordTrackers.TryGetValue(key, out var value))
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
