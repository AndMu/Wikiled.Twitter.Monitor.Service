using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Service.Tests.Logic
{
    [TestFixture]
    public class DublicateDetectorsTests
    {
        private readonly ILogger<DublicateDetectors> logger = new Logger<DublicateDetectors>(new NullLoggerFactory());

        private Mock<IMemoryCache> mockMemoryCache;

        private DublicateDetectors instance;

        [SetUp]
        public void SetUp()
        {
            mockMemoryCache = new Mock<IMemoryCache>();
            instance = CreateDublicateDetectors();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new DublicateDetectors(null, logger));
            Assert.Throws<ArgumentNullException>(() => new DublicateDetectors(mockMemoryCache.Object, null));
        }

        private DublicateDetectors CreateDublicateDetectors()
        {
            return new DublicateDetectors(mockMemoryCache.Object, logger);
        }
    }
}