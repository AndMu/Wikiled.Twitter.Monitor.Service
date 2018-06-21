using System;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface IStreamMonitor : IDisposable
    {
        ITrackingInstance Trackers { get; }
    }
}