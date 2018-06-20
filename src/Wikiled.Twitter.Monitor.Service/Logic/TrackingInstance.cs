using System;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;
using Wikiled.Common.Extensions;
using Wikiled.Twitter.Persistency;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class TrackingInstance : ITrackingInstance
    {
        private readonly TimingStreamSource streamSource;

        private readonly ISentimentAnalysis sentiment;

        private readonly TwitPersistency persistency;

        public TrackingInstance(ITrackingConfigFactory trackingConfigFactory, ISentimentAnalysis sentiment)
        {
            if (trackingConfigFactory == null)
            {
                throw new ArgumentNullException(nameof(trackingConfigFactory));
            }

            var path = trackingConfigFactory.GetPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
            }

            this.sentiment = sentiment ?? throw new ArgumentNullException(nameof(sentiment));
            Trackers = trackingConfigFactory.GetTrackers();
            Languages = trackingConfigFactory.GetLanguages();
            path.EnsureDirectoryExistence();
            streamSource = new TimingStreamSource(path, TimeSpan.FromDays(1));
            persistency = new TwitPersistency(streamSource);
        }

        public IKeywordTracker[] Trackers { get; }

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

        public void Dispose()
        {
            streamSource.Dispose();
        }
    }
}
