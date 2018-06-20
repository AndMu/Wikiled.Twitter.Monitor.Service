using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wikiled.Common.Utilities.Config;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class KeywordTracker : IKeywordTracker
    {
        private readonly IApplicationConfiguration config;

        private readonly ConcurrentQueue<(DateTime Date, double? Rating)> ratings = new ConcurrentQueue<(DateTime, double?)>();

        public KeywordTracker(IApplicationConfiguration config, string keyword)
        {
            Keyword = keyword ?? throw new ArgumentNullException(nameof(keyword));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public int TotalMessages => ratings.Count;

        public string Keyword { get; }

        public void AddRating(double? rating)
        {
            var now = config.Now;
            var yesterday = now.AddDays(-1);
            ratings.Enqueue((config.Now, rating));
            while (ratings.TryPeek(out var item) &&
                   item.Date < yesterday &&
                   ratings.TryDequeue(out item))
            {
            }
        }

        public double? AverageSentiment(int lastHours = 24)
        {
            var sentiment = GetSentiments(lastHours).ToArray();
            if (sentiment.Length == 0)
            {
                return null;
            }

            return sentiment.Average();
        }

        public int TotalWithSentiment(int lastHours = 24)
        {
            return GetSentiments(lastHours).Count();
        }

        private IEnumerable<double> GetSentiments(int lastHours = 24)
        {
            var time = config.Now;
            time = time.AddHours(-lastHours);
            return ratings.Where(item => item.Rating.HasValue && item.Date > time).Select(item => item.Rating.Value);
        }
    }
}
