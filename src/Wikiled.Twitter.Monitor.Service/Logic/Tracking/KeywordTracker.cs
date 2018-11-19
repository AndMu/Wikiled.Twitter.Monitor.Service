using System;
using Wikiled.MachineLearning.Mathematics.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class KeywordTracker : IKeywordTracker
    {
        public KeywordTracker(string keyword, bool isKeyword, ITracker tracker)
        {
            Keyword = keyword ?? throw new ArgumentNullException(nameof(keyword));
            IsKeyword = isKeyword;
            Tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        }

        public void AddRating(string text, RatingRecord record)
        {
            if (!text.Contains(Keyword, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Tracker.AddRating(record);
        }

        public string Keyword { get; }

        public bool IsKeyword { get; }

        public ITracker Tracker { get; }
    }
}
