using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Client;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class StreamApiClientFactory : IStreamApiClientFactory
    {
        private readonly SentimentConfig config;

        private ILogger<StreamApiClient> logger;

        public StreamApiClientFactory(SentimentConfig config, ILogger<StreamApiClient> logger)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IStreamApiClient Contruct()
        {
            return new StreamApiClient(new HttpClient { Timeout = TimeSpan.FromMinutes(20) }, new Uri(config.Url), logger);
        }
    }
}
