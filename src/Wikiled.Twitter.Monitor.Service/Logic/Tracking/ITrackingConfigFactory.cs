using Tweetinvi.Models;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public interface ITrackingConfigFactory
    {
        string GetPath();

        ITracker[] GetTrackers();

        LanguageFilter[] GetLanguages();
    }
}