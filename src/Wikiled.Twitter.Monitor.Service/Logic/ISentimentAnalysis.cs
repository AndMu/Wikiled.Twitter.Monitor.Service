using System.Threading.Tasks;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface ISentimentAnalysis
    {
        Task<double?> MeasureSentiment(string text);
    }
}