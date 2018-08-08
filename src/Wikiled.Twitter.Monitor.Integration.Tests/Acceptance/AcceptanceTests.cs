using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Server.Core.Testing.Server;
using Wikiled.Twitter.Monitor.Api.Service;
using Wikiled.Twitter.Monitor.Service;

namespace Wikiled.Twitter.Monitor.Integration.Tests.Acceptance
{
    [TestFixture]
    public class AcceptanceTests
    {
        private ServerWrapper wrapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            wrapper = ServerWrapper.Create<Startup>(TestContext.CurrentContext.TestDirectory, services => { });
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            wrapper.Dispose();
        }

        [Test]
        public async Task Version()
        {
            var response = await wrapper.ApiClient.GetRequest<RawResponse<string>>("api/twitter/version", CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task Measure()
        {
            var analysis = new TwitterAnalysis(new ApiClientFactory(wrapper.Client, wrapper.Client.BaseAddress));
            var result = await analysis.GetTrackingResults("APPL", CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(10, result);
        }
    }
}
