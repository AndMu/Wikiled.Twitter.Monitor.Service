using System;
using Moq;
using NUnit.Framework;
using Wikiled.Common.Utilities.Config;
using Wikiled.Twitter.Monitor.Service.Logic.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic
{
    [TestFixture]
    public class StockTrackerTests
    {
        private Mock<IApplicationConfiguration> mockApplicationConfiguration;

        private Tracker instance;

        [SetUp]
        public void SetUp()
        {
            mockApplicationConfiguration = new Mock<IApplicationConfiguration>();
            instance = CreateStockTracker();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new Tracker(null, "Test", true));
            Assert.Throws<ArgumentNullException>(() => new Tracker(mockApplicationConfiguration.Object, null, true));
            Assert.AreEqual("AAPL", instance.Value);
            Assert.AreEqual(0, instance.TotalMessages);
        }

        [Test]
        public void CalculateSentiment()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            var resultSentiment = instance.AverageSentiment();
            var resultCount = instance.TotalWithSentiment();
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);

            instance.AddRating("AAPL", null);
            resultSentiment = instance.AverageSentiment();
            resultCount = instance.TotalWithSentiment();
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);

            instance.AddRating("AAPL", 1);
            resultSentiment = instance.AverageSentiment();
            resultCount = instance.TotalWithSentiment();
            Assert.AreEqual(1, resultSentiment);
            Assert.AreEqual(1, resultCount);

            instance.AddRating("AAPL", 0);
            resultSentiment = instance.AverageSentiment();
            resultCount = instance.TotalWithSentiment();
            Assert.AreEqual(0.5, resultSentiment);
            Assert.AreEqual(2, resultCount);
        }

        [Test]
        public void CalculateSentimentExpire()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            instance.AddRating("AAPL", 1);
            instance.AddRating("AAPL", 1);
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow.AddDays(2));
            instance.AddRating("AAPL", 0);
            var resultSentiment = instance.AverageSentiment();
            var resultCount = instance.TotalWithSentiment();
            Assert.AreEqual(0, resultSentiment);
            Assert.AreEqual(1, resultCount);
        }

        [Test]
        public void CalculatedHours()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            instance.AddRating("AAPL", 1);
            instance.AddRating("AAPL", 1);
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow.AddHours(2));
            
            var resultSentiment = instance.AverageSentiment(1);
            var resultCount = instance.TotalWithSentiment(1);
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);
        }

        private Tracker CreateStockTracker()
        {
            return new Tracker(mockApplicationConfiguration.Object, "AAPL", true);
        }
    }
}