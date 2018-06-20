using System;
using System.Net.Http;
using Wikiled.Common.Net.Client;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class StreamApiClientFactory : IStreamApiClientFactory
    {
        private readonly SentimentConfig config;

        public StreamApiClientFactory(SentimentConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IStreamApiClient Contruct()
        {
            return new StreamApiClient(new HttpClient(), new Uri(config.Url));
        }
    }
}
