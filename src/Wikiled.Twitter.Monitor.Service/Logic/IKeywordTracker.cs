namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface IKeywordTracker
    {
        int TotalMessages { get; }

        string Keyword { get; }

        void AddRating(string text, double? rating);

        double? AverageSentiment(int lastHours = 24);

        int TotalWithSentiment(int lastHours = 24);
    }
}