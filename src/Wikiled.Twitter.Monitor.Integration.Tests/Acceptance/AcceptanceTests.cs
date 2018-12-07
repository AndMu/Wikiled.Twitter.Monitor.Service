using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Api.Service;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Server.Core.Testing.Server;
using Wikiled.Twitter.Monitor.Service;

namespace Wikiled.Twitter.Monitor.Integration.Tests.Acceptance
{
    [TestFixture]
    public class AcceptanceTests
    {
        private ServerWrapper wrapper;

        private SentimentTracking analysis;

        [OneTimeSetUp]
        public void SetUp()
        {
            wrapper = ServerWrapper.Create<Startup>(TestContext.CurrentContext.TestDirectory, services => { });
            analysis = new SentimentTracking(new ApiClientFactory(wrapper.Client, new Uri(wrapper.Client.BaseAddress, "api/monitor/")));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            wrapper.Dispose();
        }

        [Test]
        public async Task Version()
        {
            var response = await wrapper.ApiClient.GetRequest<RawResponse<string>>("api/monitor/version", CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task GetTrackingResults()
        {
            var result = await analysis.GetTrackingResults(new SentimentRequest("AMD"), CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("AMD", result["AMD"].Keyword);
            Assert.AreEqual(0, result["AMD"].Total);
        }

        [Test]
        public async Task GetTrackingHistory()
        {
            IDictionary<string, RatingRecord[]> result = await analysis.GetTrackingHistory(new SentimentRequest("AMD"), CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result["AMD"].Length);
        }
    }
}
