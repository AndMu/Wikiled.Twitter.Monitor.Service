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
    public class DuplicateDetectorsTests
    {
        private readonly ILogger<DuplicateDetectors> logger = new Logger<DuplicateDetectors>(new NullLoggerFactory());

        private Mock<IMemoryCache> mockMemoryCache;

        private DuplicateDetectors instance;

        [SetUp]
        public void SetUp()
        {
            mockMemoryCache = new Mock<IMemoryCache>();
            instance = CreateDuplicateDetectors();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new DuplicateDetectors(logger, null));
            Assert.Throws<ArgumentNullException>(() => new DuplicateDetectors(null, mockMemoryCache.Object));
        }

        private DuplicateDetectors CreateDuplicateDetectors()
        {
            return new DuplicateDetectors(logger, mockMemoryCache.Object);
        }
    }
}