using Tweetinvi.Models;
using Wikiled.MachineLearning.Mathematics.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public interface ITrackingConfigFactory
    {
        string GetPath();

        IKeywordTracker[] GetTrackers();

        LanguageFilter[] GetLanguages();
    }
}