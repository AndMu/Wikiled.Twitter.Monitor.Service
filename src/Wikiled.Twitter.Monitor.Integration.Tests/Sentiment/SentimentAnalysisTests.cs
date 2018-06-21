using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Integration.Tests.Sentiment
{
    [TestFixture]
    public class SentimentAnalysisTests
    {
        private readonly ILogger<StreamApiClient> logger = new Logger<StreamApiClient>(new NullLoggerFactory());

        private SentimentAnalysis instance;

        private SentimentConfig config;

        [SetUp]
        public void SetUp()
        {
            config = new SentimentConfig();
            config.Url = "http://192.168.0.200:7017/api/sentiment/";
            config.Domain = "TwitterMarket";
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public async Task SimpleTest()
        {
            var result = await instance.MeasureSentiment("Sell and short it");
            Assert.AreEqual(-1, result);
        }

        private SentimentAnalysis CreateSentimentAnalysis()
        {
            return new SentimentAnalysis(new StreamApiClientFactory(config, logger), config, new Logger<SentimentAnalysis>(new NullLoggerFactory()));
        }
    }
}