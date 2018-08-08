using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;
using Wikiled.Common.Extensions;
using Wikiled.Twitter.Monitor.Service.Logic.Sentiment;
using Wikiled.Twitter.Persistency;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class TrackingInstance : ITrackingInstance
    {
        private readonly TimingStreamSource streamSource;

        private readonly ITwitterSentimentAnalysis sentiment;

        private readonly TwitPersistency persistency;

        private readonly Dictionary<string, ITracker> keywordTrackers;

        private readonly Dictionary<string, ITracker> userTrackers;

        private ILogger<TrackingInstance> logger;

        public TrackingInstance(ITrackingConfigFactory trackingConfigFactory, ITwitterSentimentAnalysis sentiment, ILoggerFactory loggerFactory)
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

        public ITracker[] Trackers { get; }

        public LanguageFilter[] Languages { get; }

        public async Task OnReceived(ITweetDTO tweet)
        {
            try
            {
                var sentimentValue = await sentiment.MeasureSentiment(tweet.Text);
                var tweetItem = Tweet.GenerateTweetFromDTO(tweet);
                var saveTask = Task.Run(() => persistency?.Save(tweetItem, sentimentValue));
                foreach (var tracker in Trackers)
                {
                    tracker.AddRating(tweet.Text, sentimentValue);
                }

                if (userTrackers.TryGetValue(tweet.CreatedBy.Name, out var trackerUser))
                {
                    trackerUser.AddRating(tweet.CreatedBy.Name, sentimentValue);
                }

                await saveTask;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing");
            }
        }

        public ITracker Resolve(string key)
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
