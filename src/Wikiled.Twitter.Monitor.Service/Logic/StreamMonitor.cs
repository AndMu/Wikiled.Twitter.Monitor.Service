using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models.DTO;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;
using Wikiled.Twitter.Security;
using Wikiled.Twitter.Streams;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class StreamMonitor : IStreamMonitor
    {
        private readonly IAuthentication authentication;

        private readonly ILogger<StreamMonitor> logger;

        private readonly IDuplicateDetectors duplicateDetectors;

        private IMonitoringStream stream;

        private IDisposable subscription;

        public StreamMonitor(ILogger<StreamMonitor> logger,
                             IAuthentication authentication,
                             ITrackingInstance tracker,
                             IDuplicateDetectors duplicateDetectors,
                             IMonitoringStream stream)
        {
            this.authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.duplicateDetectors = duplicateDetectors ?? throw new ArgumentNullException(nameof(duplicateDetectors));
            Trackers = tracker ?? throw new ArgumentNullException(nameof(tracker));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
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
            var auth = authentication.Authenticate();
            Auth.SetUserCredentials(auth.ConsumerKey, auth.ConsumerSecret, auth.AccessToken, auth.AccessTokenSecret);
            stream.LanguageFilters = Trackers.Languages;
            subscription = stream.MessagesReceiving
                                 .ObserveOn(TaskPoolScheduler.Default)
                                 .Where(item => !duplicateDetectors.HasReceived(item.Text))
                                 .Select(Save)
                                 .Merge()
                                 .Subscribe(
                                     item => { logger.LogDebug("Processed message: {0} ({1})", item.Text, item.Language); },
                                     e => logger.LogError(e, "Stream error"),
                                     () => logger.LogInformation("Completed!"));
            var keywords = Trackers.Trackers.Where(item => item.IsKeyword).Select(item => item.Keyword).ToArray();
            var users = Trackers.Trackers.Where(item => !item.IsKeyword).Select(item => item.Keyword).ToArray();
            Task.Factory.StartNew(async () => await stream.Start(keywords, users).ConfigureAwait(false), TaskCreationOptions.LongRunning);
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