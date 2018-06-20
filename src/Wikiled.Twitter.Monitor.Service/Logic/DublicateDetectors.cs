using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NLog;
using Wikiled.Text.Analysis.Twitter;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class DublicateDetectors : IDublicateDetectors
    {
        private readonly IMemoryCache cache;

        private readonly ILogger<DublicateDetectors> logger;

        private readonly MessageCleanup cleanup = new MessageCleanup();

        public DublicateDetectors(IMemoryCache cache, ILogger<DublicateDetectors> logger)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool HasReceived(string text)
        {
            text = cleanup.Cleanup(text);
            if (cache.TryGetValue(text, out bool _))
            {
                logger.LogDebug("Found dublicate: {0}", text);
                return true;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20));
            cache.Set(text, true, cacheEntryOptions);
            return false;
        }
    }
}
