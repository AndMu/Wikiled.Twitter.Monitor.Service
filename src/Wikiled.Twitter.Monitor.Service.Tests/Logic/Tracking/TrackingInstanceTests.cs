using System;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
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

        private Mock<ITrackingManager> manager;

        [SetUp]
        public void SetUp()
        {
            mockSentimentAnalysis = new Mock<ISentimentAnalysis>();
            manager = new Mock<ITrackingManager>();
            var tracker = new Mock<ITracker>();
            manager.Setup(item => item.Resolve(It.IsAny<string>(), It.IsAny<string>())).Returns(tracker.Object);
            var config = new TwitterConfig();
            config.Keywords = new[] { "Test" };
            config.HashKeywords = true;
            config.Persistency = "Test";
            trackingConfigFactory = new TrackingConfigFactory(
                new NullLogger<TrackingConfigFactory>(), 
                config,
                manager.Object);            
            instance = CreateInstance();
        }

        [Test]
        public void Trackers()
        {
            Assert.AreEqual(1, instance.Trackers.Length);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(
                () => new TrackingInstance(
                    null,
                    trackingConfigFactory,
                    mockSentimentAnalysis.Object,
                    manager.Object));
            Assert.Throws<ArgumentNullException>(
                () => new TrackingInstance(
                    new NullLogger<TrackingInstance>(),
                    null,
                    mockSentimentAnalysis.Object,
                    manager.Object));
            Assert.Throws<ArgumentNullException>(
                () => new TrackingInstance(
                    new NullLogger<TrackingInstance>(),
                    trackingConfigFactory,
                    null,
                    manager.Object));
            Assert.Throws<ArgumentNullException>(
                () => new TrackingInstance(
                    new NullLogger<TrackingInstance>(),
                    trackingConfigFactory,
                    mockSentimentAnalysis.Object,
                    null));
        }

        private TrackingInstance CreateInstance()
        {
            return new TrackingInstance(
                new NullLogger<TrackingInstance>(),
                trackingConfigFactory,
                mockSentimentAnalysis.Object,
                manager.Object);
        }
    }
}
