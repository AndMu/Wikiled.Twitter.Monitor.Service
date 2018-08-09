﻿using System.Threading;
using System.Threading.Tasks;
using Wikiled.Twitter.Monitor.Api.Response;

namespace Wikiled.Twitter.Monitor.Api.Service
{
    public interface ITwitterAnalysis
    {
        Task<TrackingResults> GetTrackingResults(string keyword, CancellationToken token);
    }
}