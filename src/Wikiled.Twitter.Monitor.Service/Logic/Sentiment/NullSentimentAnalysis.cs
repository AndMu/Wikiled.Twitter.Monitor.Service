using System.Threading.Tasks;

namespace Wikiled.Twitter.Monitor.Service.Logic.Sentiment
{
    public class NullSentimentAnalysis : ITwitterSentimentAnalysis
    {
        public Task<double?> MeasureSentiment(string text)
        {
            return Task.FromResult<double?>(null);
        }
    }
}
