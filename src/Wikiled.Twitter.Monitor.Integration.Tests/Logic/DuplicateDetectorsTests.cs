using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Integration.Tests.Logic
{
    [TestFixture]
    public class DuplicateDetectorsTests
    {
        private DuplicateDetectors instance;

        private MemoryCache cache;

        private readonly ILogger<DuplicateDetectors> logger = new Logger<DuplicateDetectors>(new NullLoggerFactory());

        [SetUp]
        public void SetUp()
        {
            cache = new MemoryCache(new MemoryCacheOptions());
            instance = CreateDuplicateDetectors();
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

        private DuplicateDetectors CreateDuplicateDetectors()
        {
            return new DuplicateDetectors(logger, cache);
        }
    }
}