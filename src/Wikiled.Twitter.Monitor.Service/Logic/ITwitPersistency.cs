using Tweetinvi.Models;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface ITwitPersistency
    {
        void Save(ITweet message, double? sentiment);
    }
}