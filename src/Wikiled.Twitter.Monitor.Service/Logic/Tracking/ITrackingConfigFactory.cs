using Tweetinvi.Models;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public interface ITrackingConfigFactory
    {
        TwitterConfig Config { get; }

        IKeywordTracker[] GetTrackers();

        LanguageFilter[] GetLanguages();
    }
}