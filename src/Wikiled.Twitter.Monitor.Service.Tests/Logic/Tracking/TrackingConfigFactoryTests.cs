using System;
using Moq;
using NUnit.Framework;
using Wikiled.Common.Utilities.Config;
using Wikiled.Twitter.Monitor.Service.Configuration;
using Wikiled.Twitter.Monitor.Service.Logic;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic
{
    [TestFixture]
    public class TrackingConfigFactoryTests
    {
        private TwitterConfig config;

        private Mock<IApplicationConfiguration> mockApplicationConfiguration;

        private TrackingConfigFactory instance;

        [SetUp]
        public void SetUp()
        {
            config = new TwitterConfig();
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
            config.Keywords = new [] {"Test"};
            config.Users = new[] { "@Test" };
            var result = instance.GetTrackers();
            Assert.AreEqual(2, result.Length);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new TrackingConfigFactory(
                null,
                mockApplicationConfiguration.Object));
            Assert.Throws<ArgumentNullException>(() => new TrackingConfigFactory(
                config,
                null));
        }

        private TrackingConfigFactory CreateFactory()
        {
            return new TrackingConfigFactory(
                config,
                mockApplicationConfiguration.Object);
        }
    }
}