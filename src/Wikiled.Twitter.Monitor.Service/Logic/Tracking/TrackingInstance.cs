using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;
using Wikiled.Sentiment.Api.Service;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class TrackingInstance : ITrackingInstance
    {
        private readonly ISentimentAnalysis sentiment;

        private readonly ITwitPersistency persistency;

        private readonly ILogger<TrackingInstance> logger;

        private readonly ITrackingManager manager;

        private readonly HashSet<string> users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public TrackingInstance(ILogger<TrackingInstance> logger, ITrackingConfigFactory trackingConfigFactory, ISentimentAnalysis sentiment, ITrackingManager manager, ITwitPersistency persistency)
        {
            if (trackingConfigFactory == null)
            {
                throw new ArgumentNullException(nameof(trackingConfigFactory));
            }

            this.sentiment = sentiment ?? throw new ArgumentNullException(nameof(sentiment));
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.persistency = persistency ?? throw new ArgumentNullException(nameof(persistency));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }
}
