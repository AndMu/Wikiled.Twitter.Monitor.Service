﻿using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models.DTO;
using Wikiled.Twitter.Security;
using Wikiled.Twitter.Streams;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class StreamMonitor : IStreamMonitor
    {
        private readonly IAuthentication authentication;

        private readonly ILogger<StreamMonitor> logger;

        private readonly IDublicateDetectors dublicateDetectors;

        private MonitoringStream stream;

        private IDisposable subscription;

        public StreamMonitor(IAuthentication authentication, ITrackingInstance tracker, IDublicateDetectors dublicateDetectors, ILogger<StreamMonitor> logger)
        {
            this.authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dublicateDetectors = dublicateDetectors ?? throw new ArgumentNullException(nameof(dublicateDetectors));
            Trackers = tracker ?? throw new ArgumentNullException(nameof(tracker));
            Start();
        }

        public ITrackingInstance Trackers { get; }

        public void Dispose()
        {
            Stop();
        }

        private void Start()
        {
            logger.LogInformation("Starting stream...");
            stream = new MonitoringStream(authentication);
            stream.LanguageFilters = Trackers.Languages;
            subscription = stream.MessagesReceiving
                .ObserveOn(TaskPoolScheduler.Default)
                .Where(item => !dublicateDetectors.HasReceived(item.Text))
                .Select(Save)
                .Merge()
                .Subscribe(item => { logger.LogDebug("Processed message: {0}", item.Text); });

            Task.Factory.StartNew(
                async () => await stream.Start(Trackers.Trackers.Select(item => item.Keyword).ToArray(),
                    new string[] { }), TaskCreationOptions.LongRunning);
        }

        private void Stop()
        {
            logger.LogInformation("Stopping stream...");
            subscription?.Dispose();
            subscription = null;
            stream?.Dispose();
            stream = null;
        }

        private async Task<ITweetDTO> Save(ITweetDTO tweet)
        {
            await Task.Run(() => Trackers.OnReceived(tweet)).ConfigureAwait(false);
            return tweet;
        }
    }
}