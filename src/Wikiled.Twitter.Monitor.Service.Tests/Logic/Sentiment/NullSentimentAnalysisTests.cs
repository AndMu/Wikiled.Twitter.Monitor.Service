using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Twitter.Monitor.Service.Logic.Sentiment;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic.Sentiment
{
    [TestFixture]
    public class NullSentimentAnalysisTests
    {
        private NullSentimentAnalysis instance;

        [SetUp]
        public void SetUp()
        {
            instance = CreateInstance();
        }

        [Test]
        public async Task Construct()
        {
            var result = await instance.MeasureSentiment("Test");
            Assert.IsNull(result);
        }

        private NullSentimentAnalysis CreateInstance()
        {
            return new NullSentimentAnalysis();
        }
    }
}
