using NUnit.Framework;
using Wikiled.Twitter.Monitor.Api.Response;

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
            results.Sentiment["2H"] = new SentimentResult
            {
                AverageSentiment = 2,
                TotalMessages = 2
            };

            results.Sentiment["24H"] = new SentimentResult();
            Assert.AreEqual("Tracking Result: [Test](2) [2H]:Average Sentiment: 2(2) [24H]:Average Sentiment: (0)", results.ToString());
        }
    }
}
