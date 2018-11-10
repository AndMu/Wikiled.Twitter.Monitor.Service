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

        [TestCase("Test", "Test", false, 1)]
        [TestCase("Test", "@Test", false, 2)]
        [TestCase("Test", "@Test", true, 3)]
        [TestCase("#Test", "@Test", true, 2)]
        public void GetTrackers(string keyword, string user, bool hashkey, int expected)
        {
            config.HashKeywords = hashkey;
            config.Keywords = new[] { keyword };
            config.Users = new[] { user };
            var result = instance.GetTrackers();
            Assert.AreEqual(expected, result.Length);
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