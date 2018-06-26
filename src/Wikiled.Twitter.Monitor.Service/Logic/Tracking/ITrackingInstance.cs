using System;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public interface ITrackingInstance : IDisposable
    {
        ITracker[] KeywordTrackers { get; }

        LanguageFilter[] Languages { get; }

        Task OnReceived(ITweetDTO tweet);

        ITracker Resolve(string key);
    }
}