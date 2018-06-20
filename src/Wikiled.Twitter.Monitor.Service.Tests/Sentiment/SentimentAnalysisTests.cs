using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Service.Tests.Sentiment
{
    [TestFixture]
    public class SentimentAnalysisTests
    {
        private Mock<IStreamApiClientFactory> mockStreamApiClient;

        private readonly ILogger<SentimentAnalysis> logger = new Logger<SentimentAnalysis>(new NullLoggerFactory());

        private SentimentAnalysis instance;

        private SentimentConfig config;

        [SetUp]
        public void SetUp()
        {
            mockStreamApiClient = new Mock<IStreamApiClientFactory>();
            config = new SentimentConfig();
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new SentimentAnalysis(null, config, logger));
            Assert.Throws<ArgumentNullException>(() => new SentimentAnalysis(mockStreamApiClient.Object, null, logger));
            Assert.Throws<ArgumentNullException>(() => new SentimentAnalysis(mockStreamApiClient.Object, config, null));
        }

        private SentimentAnalysis CreateSentimentAnalysis()
        {
            return new SentimentAnalysis(mockStreamApiClient.Object, config, logger);
        }
    }
}