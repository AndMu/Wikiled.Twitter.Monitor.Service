using System;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Service.Tests.Sentiment
{
    [TestFixture]
    public class DublicateDetectorsTests
    {
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
            Assert.Throws<ArgumentNullException>(() => new DublicateDetectors(null));
        }

        private DublicateDetectors CreateDublicateDetectors()
        {
            return new DublicateDetectors(mockMemoryCache.Object);
        }
    }
}