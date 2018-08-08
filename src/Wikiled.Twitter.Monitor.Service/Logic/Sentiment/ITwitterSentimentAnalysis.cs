using System.Threading.Tasks;

namespace Wikiled.Twitter.Monitor.Service.Logic.Sentiment
{
    public interface ITwitterSentimentAnalysis
    {
        Task<double?> MeasureSentiment(string text);
    }
}