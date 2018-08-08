using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.MachineLearning.Mathematics;
using Wikiled.Sentiment.Api.Service;

namespace Wikiled.Twitter.Monitor.Service.Logic.Sentiment
{
    public class TwitterSentimentAnalysis : ITwitterSentimentAnalysis
    {
        private readonly ILogger<TwitterSentimentAnalysis> logger;

        private readonly ISentimentAnalysis sentiment;

        public TwitterSentimentAnalysis(ISentimentAnalysis sentiment, ILoggerFactory logger)
        {
            this.logger = logger?.CreateLogger<TwitterSentimentAnalysis>() ?? throw new ArgumentNullException(nameof(logger));
            this.sentiment = sentiment ?? throw new ArgumentNullException(nameof(sentiment));
        }

        public async Task<double?> MeasureSentiment(string text)
        {
            logger.LogDebug("MeasureSentiment");
            try
            {
                var result = await sentiment.Measure(text, CancellationToken.None).ConfigureAwait(false);
                if (result == null)
                {
                    logger.LogWarning("No meaningful response");
                    return null;
                }

                logger.LogDebug("MeasureSentiment Calculated: {0}", result.Stars);
                return result.Stars.HasValue ? RatingCalculator.ConvertToRaw(result.Stars.Value) : result.Stars;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed sentiment processing");
                return null;
            }
        }
    }
}
