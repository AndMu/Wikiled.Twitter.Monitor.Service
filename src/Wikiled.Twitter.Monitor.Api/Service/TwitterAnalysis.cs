using System;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Common.Net.Client;
using Wikiled.Twitter.Monitor.Api.Response;

namespace Wikiled.Twitter.Monitor.Api.Service
{
    public class TwitterAnalysis : ITwitterAnalysis
    {
        private readonly IApiClient client;

        public TwitterAnalysis(IApiClientFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            client = factory.GetClient();
            client = client ?? throw new ArgumentNullException(nameof(client));
        }


        public async Task<TrackingResults> GetTrackingResults(string keyword, CancellationToken token)
        {
            if (keyword is null)
            {
                throw new ArgumentNullException(nameof(keyword));
            }

            var result = await client.GetRequest<RawResponse<TrackingResults>>($"api/twitter/{keyword}", token).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                throw new ApplicationException("Failed to retrieve:" + result.HttpResponseMessage);
            }

            return result.Result.Value;
        }
    }
}
