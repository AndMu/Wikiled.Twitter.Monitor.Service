using NUnit.Framework;
using Wikiled.MachineLearning.Mathematics.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Tests.Response
{
    [TestFixture]
    public class TrackingResultsTests
    {
        [Test]
        public void String()
        {
            var results = new TrackingResults();
            Assert.AreEqual("Tracking Result: [](0)", results.ToString());
            results.Keyword = "Test";
            results.Total = 2;
            results.Sentiment["2H"] = new TrackingResult
            {
                Average = 2,
                TotalMessages = 2
            };

            results.Sentiment["24H"] = new TrackingResult();
            Assert.AreEqual("Tracking Result: [Test](2) [2H]:Average Sentiment: 2(2) [24H]:Average Sentiment: (0)", results.ToString());
        }
    }
}
