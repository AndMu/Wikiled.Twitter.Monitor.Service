using Wikiled.Common.Net.Client;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface IStreamApiClientFactory
    {
        IStreamApiClient Contruct();
    }
}
