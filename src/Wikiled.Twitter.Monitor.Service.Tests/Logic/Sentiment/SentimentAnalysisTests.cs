using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Wikiled.Sentiment.Api.Service;
using Wikiled.Twitter.Monitor.Service.Logic.Sentiment;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic.Sentiment
{
    [TestFixture]
    public class SentimentAnalysisTests
    {
        private Mock<ISentimentAnalysis> analysis;

        private readonly ILoggerFactory logger = new NullLoggerFactory();

        private TwitterSentimentAnalysis instance;

        [SetUp]
        public void SetUp()
        {
            analysis = new Mock<ISentimentAnalysis>();
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new TwitterSentimentAnalysis(null, logger));
            Assert.Throws<ArgumentNullException>(() => new TwitterSentimentAnalysis(analysis.Object, null));
        }

        private TwitterSentimentAnalysis CreateSentimentAnalysis()
        {
            return new TwitterSentimentAnalysis(analysis.Object, logger);
        }
    }
}