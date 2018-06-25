namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public interface ITracker
    {
        int TotalMessages { get; }

        string Value { get; }

        bool IsKeyword { get; }

        void AddRating(string text, double? rating);

        double? AverageSentiment(int lastHours = 24);

        int TotalWithSentiment(int lastHours = 24);
    }
}