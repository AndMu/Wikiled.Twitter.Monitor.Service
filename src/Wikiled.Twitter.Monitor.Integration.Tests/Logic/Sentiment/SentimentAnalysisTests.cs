using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Sentiment.Api.Request;
using Wikiled.Sentiment.Api.Service;
using Wikiled.Twitter.Monitor.Service.Logic.Sentiment;

namespace Wikiled.Twitter.Monitor.Integration.Tests.Logic.Sentiment
{
    [TestFixture]
    public class SentimentAnalysisTests
    {
        private TwitterSentimentAnalysis instance;

        [SetUp]
        public void SetUp()
        {
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public async Task SimpleTest()
        {
            var result = await instance.MeasureSentiment("Sell and short it").ConfigureAwait(false);
            Assert.AreEqual(-1, result);
        }

        private TwitterSentimentAnalysis CreateSentimentAnalysis()
        {
            return new TwitterSentimentAnalysis(
                new SentimentAnalysis(new StreamApiClientFactory(new NullLoggerFactory(),
                                                                 new HttpClient { Timeout = TimeSpan.FromSeconds(20) },
                                                                 new Uri("http://192.168.0.70:7017")),
                                      new WorkRequest
                                      {
                                          Domain = "TwitterMarket",
                                          CleanText = true
                                      }),
                new NullLoggerFactory());
        }
    }
}