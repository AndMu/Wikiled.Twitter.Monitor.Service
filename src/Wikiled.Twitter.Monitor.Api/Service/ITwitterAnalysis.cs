using System.Threading;
using System.Threading.Tasks;
using Wikiled.MachineLearning.Mathematics.Tracking;

namespace Wikiled.Twitter.Monitor.Api.Service
{
    public interface ITwitterAnalysis
    {
        Task<TrackingResults> GetTrackingResults(string keyword, CancellationToken token);
    }
}
