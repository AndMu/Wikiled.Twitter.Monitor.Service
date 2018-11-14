using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Wikiled.Common.Utilities.Config;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic
{
    [TestFixture]
    public class StockTrackerTests
    {
        private Mock<IApplicationConfiguration> mockApplicationConfiguration;

        private KeywordTracker instance;

        private ILoggerFactory loggerFactory;

        [SetUp]
        public void SetUp()
        {
            loggerFactory = new NullLoggerFactory();
            mockApplicationConfiguration = new Mock<IApplicationConfiguration>();
            instance = CreateStockTracker();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new KeywordTracker(null, loggerFactory, "Test", true));
            Assert.Throws<ArgumentNullException>(() => new KeywordTracker(mockApplicationConfiguration.Object, loggerFactory, null, true));
            Assert.Throws<ArgumentNullException>(() => new KeywordTracker(mockApplicationConfiguration.Object, null, "test", true));
            Assert.AreEqual("AAPL", instance.Keyword);
        }

        [Test]
        public void CalculateSentiment()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            var resultSentiment = instance.Tracker.AverageSentiment();
            var resultCount = instance.Tracker.Count();
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);

            instance.Tracker.AddRating(new RatingRecord("1", DateTime.UtcNow, null));
            resultSentiment = instance.Tracker.AverageSentiment();
            resultCount = instance.Tracker.Count();
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);

            instance.Tracker.AddRating(new RatingRecord("2", DateTime.UtcNow, 1));
            resultSentiment = instance.Tracker.AverageSentiment();
            resultCount = instance.Tracker.Count();
            Assert.AreEqual(1, resultSentiment);
            Assert.AreEqual(1, resultCount);

            instance.Tracker.AddRating(new RatingRecord("3", DateTime.UtcNow, 0));
            resultSentiment = instance.Tracker.AverageSentiment();
            resultCount = instance.Tracker.Count();
            Assert.AreEqual(0.5, resultSentiment);
            Assert.AreEqual(2, resultCount);
        }

        [Test]
        public void CalculateSentimentExpire()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            instance.Tracker.AddRating(new RatingRecord("1", DateTime.UtcNow, 1));
            instance.Tracker.AddRating(new RatingRecord("2", DateTime.UtcNow, 1));
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow.AddDays(2));
            instance.Tracker.AddRating(new RatingRecord("3", DateTime.UtcNow, 0));
            var resultSentiment = instance.Tracker.AverageSentiment();
            var resultCount = instance.Tracker.Count(false);
            Assert.AreEqual(null, resultSentiment);
            Assert.AreEqual(0, resultCount);
        }

        [Test]
        public void CalculatedHours()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            instance.Tracker.AddRating(new RatingRecord("1", DateTime.UtcNow, 1));
            instance.Tracker.AddRating(new RatingRecord("2", DateTime.UtcNow, 1));
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow.AddHours(2));
            
            var resultSentiment = instance.Tracker.AverageSentiment(1);
            var resultCount = instance.Tracker.Count(lastHours: 1);
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);
        }

        private KeywordTracker CreateStockTracker()
        {
            return new KeywordTracker(mockApplicationConfiguration.Object, loggerFactory, "AAPL", true);
        }
    }
}