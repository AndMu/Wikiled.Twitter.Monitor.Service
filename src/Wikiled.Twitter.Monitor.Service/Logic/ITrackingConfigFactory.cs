using Tweetinvi.Models;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface ITrackingConfigFactory
    {
        string GetPath();

        IKeywordTracker[] GetTrackers();

        LanguageFilter[] GetLanguages();
    }
}