using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Client;
using Wikiled.Sentiment.Api.Request;
using Wikiled.Sentiment.Text.Data.Review;
using Wikiled.Sentiment.Text.Sentiment;
using Wikiled.Text.Analysis.Structure;
using Wikiled.Twitter.Monitor.Service.Configuration;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class SentimentAnalysis : ISentimentAnalysis
    {
        private readonly ILogger<SentimentAnalysis> logger;

        private readonly IStreamApiClient client;

        private readonly SentimentConfig config;

        public SentimentAnalysis(IStreamApiClientFactory factory, SentimentConfig config, ILogger<SentimentAnalysis> logger)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            client = factory.Contruct();
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<double?> MeasureSentiment(string text)
        {
            logger.LogDebug("MeasureSentiment");
            WorkRequest request = new WorkRequest();
            request.CleanText = true;
            request.Documents = new[] { new SingleProcessingData(text) };
            request.Domain = config.Domain;
            var result = await client.PostRequest<WorkRequest, Document>("parsestream", request, CancellationToken.None).LastOrDefaultAsync();
            logger.LogDebug("MeasureSentiment Calculated: {0}", result.Stars);
            return result.Stars.HasValue ? RatingCalculator.ConvertToRaw(result.Stars.Value) : result.Stars;
        }
    }
}
