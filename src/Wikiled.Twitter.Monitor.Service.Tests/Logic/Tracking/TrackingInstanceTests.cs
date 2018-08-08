using System;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NLog.Extensions.Logging;
using NUnit.Framework;
using Wikiled.Common.Utilities.Config;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic.Sentiment;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic.Tracking
{
    [TestFixture]
    public class TrackingInstanceTests
    {
        private TrackingConfigFactory trackingConfigFactory;

        private Mock<ITwitterSentimentAnalysis> mockSentimentAnalysis;

        private TrackingInstance instance;

        [SetUp]
        public void SetUp()
        {
            var config = new TwitterConfig();
            config.Keywords = new[] { "Test" };
            config.HashKeywords = true;
            config.Persistency = "Test";
            trackingConfigFactory = new TrackingConfigFactory(config, new ApplicationConfiguration(), new NullLogger<TrackingConfigFactory>());
            mockSentimentAnalysis = new Mock<ITwitterSentimentAnalysis>();
            instance = CreateInstance();
        }

        [Test]
        public void Trackers()
        {
            Assert.AreEqual(2, instance.Trackers.Length);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new TrackingInstance(
                null,
                mockSentimentAnalysis.Object,
                new NLogLoggerFactory()));
            Assert.Throws<ArgumentNullException>(() => new TrackingInstance(
                trackingConfigFactory,
                null,
                new NLogLoggerFactory()));
            Assert.Throws<ArgumentNullException>(() => new TrackingInstance(
                trackingConfigFactory,
                mockSentimentAnalysis.Object,
                null));
        }

        private TrackingInstance CreateInstance()
        {
            return new TrackingInstance(
                trackingConfigFactory,
                mockSentimentAnalysis.Object,
                new NullLoggerFactory());
        }
    }
}
