using System;
using System.Reactive.Concurrency;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NLog.Extensions.Logging;
using NUnit.Framework;
using Wikiled.Common.Utilities.Config;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Sentiment.Api.Service;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic.Tracking
{
    [TestFixture]
    public class TrackingInstanceTests
    {
        private TrackingConfigFactory trackingConfigFactory;

        private Mock<ISentimentAnalysis> mockSentimentAnalysis;

        private TrackingInstance instance;

        [SetUp]
        public void SetUp()
        {
            var config = new TwitterConfig();
            config.Keywords = new[] { "Test" };
            config.HashKeywords = true;
            config.Persistency = "Test";
            trackingConfigFactory = new TrackingConfigFactory(
                new NullLoggerFactory(), 
                config,
                new ApplicationConfiguration(),
                new ExpireTracking(TaskPoolScheduler.Default,
                                   new NullLogger<ExpireTracking>(),
                                   new TrackingConfiguration(TimeSpan.FromMinutes(10), TimeSpan.FromDays(1))));
            mockSentimentAnalysis = new Mock<ISentimentAnalysis>();
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
