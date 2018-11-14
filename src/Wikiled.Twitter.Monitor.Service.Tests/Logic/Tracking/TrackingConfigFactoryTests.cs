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

        private Mock<IApplicationConfiguration> mockApplicationConfiguration;

        private Mock<IExpireTracking> expireTracking;

        private TrackingConfigFactory instance;

        private ILoggerFactory logger;

        [SetUp]
        public void SetUp()
        {
            logger = new NullLoggerFactory();
            config = new TwitterConfig();
            expireTracking = new Mock<IExpireTracking>();
            mockApplicationConfiguration = new Mock<IApplicationConfiguration>();
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
                                                     mockApplicationConfiguration.Object,
                                                     expireTracking.Object));

            Assert.Throws<ArgumentNullException>(() => new TrackingConfigFactory(
                                                     logger,
                                                     null,
                                                     mockApplicationConfiguration.Object,
                                                     expireTracking.Object));

            Assert.Throws<ArgumentNullException>(() => new TrackingConfigFactory(
                                                     logger,
                                                     config,
                                                     null,
                                                     expireTracking.Object));

            Assert.Throws<ArgumentNullException>(() => new TrackingConfigFactory(
                                                     logger,
                                                     config,
                                                     mockApplicationConfiguration.Object,
                                                     null));
        }

        private TrackingConfigFactory CreateFactory()
        {
            return new TrackingConfigFactory(
                logger,
                config,
                mockApplicationConfiguration.Object,
                expireTracking.Object);
        }
    }
}