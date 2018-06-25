using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly ISentimentAnalysis sentiment;

        private readonly TwitPersistency persistency;

        private readonly Dictionary<string, ITracker> trackers;

        public TrackingInstance(ITrackingConfigFactory trackingConfigFactory, ISentimentAnalysis sentiment)
        {
            if (trackingConfigFactory == null)
            {
                throw new ArgumentNullException(nameof(trackingConfigFactory));
            }

            var path = trackingConfigFactory.GetPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Value cannot be null or whitespace", nameof(path));
            }

            this.sentiment = sentiment ?? throw new ArgumentNullException(nameof(sentiment));
            Trackers = trackingConfigFactory.GetTrackers();
            Languages = trackingConfigFactory.GetLanguages();
            path.EnsureDirectoryExistence();
            streamSource = new TimingStreamSource(path, TimeSpan.FromDays(1));
            persistency = new TwitPersistency(streamSource);
            trackers = Trackers.ToDictionary(item => item.Value, item => item, StringComparer.OrdinalIgnoreCase);
        }

        public ITracker[] Trackers { get; }

        public LanguageFilter[] Languages { get; }

        public async Task OnReceived(ITweetDTO tweet)
        {
            var sentimentValue = await sentiment.MeasureSentiment(tweet.Text);
            var saveTask = Task.Run(() => persistency?.Save(tweet, sentimentValue));
            foreach (var tracker in Trackers)
            {
                tracker.AddRating(tweet.Text, sentimentValue);
            }

            await saveTask;
        }

        public ITracker Resolve(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            trackers.TryGetValue(key, out var value);
            return value;
        }

        public void Dispose()
        {
            streamSource.Dispose();
        }
    }
}
