using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Integration.Tests.Logic
{
    [TestFixture]
    public class DublicateDetectorsTests
    {
        private DublicateDetectors instance;

        private MemoryCache cache;

        private readonly ILogger<DublicateDetectors> logger = new Logger<DublicateDetectors>(new NullLoggerFactory());

        [SetUp]
        public void SetUp()
        {
            cache = new MemoryCache(new MemoryCacheOptions());
            instance = CreateDublicateDetectors();
        }

        [TearDown]
        public void TestCleanup()
        {
            cache.Dispose();
        }

        [Test]
        public void Construct()
        {
            var result = instance.HasReceived(@"Test Message http://dadas.com");
            Assert.IsFalse(result);
            result = instance.HasReceived(@"Test Message http://dadaczxczxs.com");
            Assert.IsTrue(result);
            result = instance.HasReceived(@"Test Message 3");
            Assert.IsFalse(result);
        }

        private DublicateDetectors CreateDublicateDetectors()
        {
            return new DublicateDetectors(cache, logger);
        }
    }
}