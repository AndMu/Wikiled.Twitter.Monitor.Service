using System;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public interface ITrackingInstance : IDisposable
    {
        IKeywordTracker[] Trackers { get; }

        LanguageFilter[] Languages { get; }

        Task OnReceived(ITweetDTO tweet);
    }
}