using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Wikiled.Common.Utilities.Config;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic.Tracking
{
    [TestFixture]
    public class TrackingConfigFactoryTests
    {
        private TwitterConfig config;

        private Mock<ITrackingManager> tracking;

        private TrackingConfigFactory instance;

        private ILogger<TrackingConfigFactory> logger;

        [SetUp]
        public void SetUp()
        {
            logger = new NullLogger<TrackingConfigFactory>();
            config = new TwitterConfig();
            tracking = new Mock<ITrackingManager>();
            var tracker = new Mock<ITracker>();
            tracking.Setup(item => item.Resolve(It.IsAny<string>(), It.IsAny<string>())).Returns(tracker.Object);
            instance = CreateFactory();
        }

        [Test]
        public void GetLanguagesEmpty()
        {
            var result = instance.GetLanguages();
            Assert.IsNull(result);
        }

        [Test]
        public void GetTrackersEmpty()
        {
            var result = instance.GetTrackers();
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void GetTrackers()
        {
            config.HashKeywords = true;
            config.Keywords = new[] { "Test" };
            config.Users = new[] { "@Test" };
            var result = instance.GetTrackers();
            Assert.AreEqual(2, result.Length);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new TrackingConfigFactory(
                                                     null,
                                                     config,
                                                     tracking.Object));

            Assert.Throws<ArgumentNullException>(() => new TrackingConfigFactory(
                                                     logger,
                                                     null,
                                                     tracking.Object));

            Assert.Throws<ArgumentNullException>(() => new TrackingConfigFactory(
                                                     logger,
                                                     config,
                                                     null));
        }

        private TrackingConfigFactory CreateFactory()
        {
            return new TrackingConfigFactory(
                logger,
                config,
                tracking.Object);
        }
    }
}