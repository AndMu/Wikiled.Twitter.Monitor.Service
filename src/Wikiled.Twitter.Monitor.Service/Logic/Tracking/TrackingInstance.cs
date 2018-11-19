using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly ILogger<TrackingInstance> logger;

        private readonly ITrackingManager manager;

        private readonly HashSet<string> users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public TrackingInstance(ILogger<TrackingInstance> logger, ITrackingConfigFactory trackingConfigFactory, ISentimentAnalysis sentiment, ITrackingManager manager)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (trackingConfigFactory == null)
            {
                throw new ArgumentNullException(nameof(trackingConfigFactory));
            }

            string path = trackingConfigFactory.Config.Persistency;
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Value cannot be null or whitespace", nameof(path));
            }

            this.sentiment = sentiment ?? throw new ArgumentNullException(nameof(sentiment));
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.logger = logger;
            Trackers = trackingConfigFactory.GetTrackers();
            foreach (var tracker in Trackers.Where(item => !item.IsKeyword))
            {
                if (users.Contains(tracker.Keyword))
                {
                    logger.LogWarning("Keyword is already added {0}", tracker.Keyword);
                    continue;
                }

                users.Add(tracker.Keyword);
            }

            Languages = trackingConfigFactory.GetLanguages();
            path.EnsureDirectoryExistence();
            streamSource = new TimingStreamSource(path, TimeSpan.FromDays(1));
            persistency = new TwitPersistency(streamSource);
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
                RatingRecord rating = new RatingRecord(tweet.Id.ToString(), DateTime.UtcNow, sentimentValue);
                foreach (IKeywordTracker tracker in Trackers)
                {
                    tracker.AddRating(tweet.Text, rating);
                }

                if (users.Contains(tweet.CreatedBy.Name))
                {
                    manager.Resolve(tweet.CreatedBy.Name, "User").AddRating(rating);
                }

                await saveTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing");
            }
        }

        public void Dispose()
        {
            streamSource.Dispose();
        }
    }
}
