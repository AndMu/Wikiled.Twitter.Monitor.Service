using System;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface IStreamMonitor : IDisposable
    {
        ITrackingInstance Trackers { get; }
    }
}