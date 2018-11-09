using Wikiled.MachineLearning.Mathematics.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public interface IKeywordTracker
    {
        string Value { get; }

        bool IsKeyword { get; }

        ITracker Tracker { get; }

    }
}