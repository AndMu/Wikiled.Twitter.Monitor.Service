using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public interface IKeywordTracker
    {
        string Keyword { get; }

        bool IsKeyword { get; }

        void AddRating(string text, RatingRecord record);
    }
}