using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using Wikiled.Server.Core.ActionFilters;
using Wikiled.Server.Core.Helpers;
using Wikiled.Twitter.Monitor.Api.Response;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Service.Controllers
{
    [Route("api/[controller]")]
    [TypeFilter(typeof(RequestValidationAttribute))]
    public class TwitterController : Controller
    {
        private readonly ILogger<TwitterController> logger;

        private readonly IIpResolve resolve;

        private readonly IStreamMonitor monitor;

        public TwitterController(ILogger<TwitterController> logger, IIpResolve resolve, IStreamMonitor monitor)
        {
            this.resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("sentiment/{keyword}")]
        [HttpGet]
        public IActionResult GetResult(string keyword)
        {
            logger.LogInformation("GetResult [{0}] with <{1}> keyword", resolve.GetRequestIp(), keyword);
            Logic.Tracking.IKeywordTracker tracker = monitor.Trackers.Resolve(keyword);
            if (tracker == null)
            {
                logger.LogWarning("Unknown keyword + " + keyword);
                return NotFound("Unknown keyword + " + keyword);
            }

            TrackingResults result = new TrackingResults
            {
                Keyword = keyword
            };
            int[] steps = { 24, 12, 6, 1 };
            foreach (int step in steps)
            {
                result.Sentiment[$"{step}H"] = new SentimentResult { AverageSentiment = tracker.Tracker.AverageSentiment(step), TotalMessages = tracker.Tracker.Count(lastHours: step) };
            }

            result.Total = tracker.Tracker.Count(false);
            return Ok(result);
        }

        [Route("version")]
        [HttpGet]
        public string ServerVersion()
        {
            string version = $"Version: [{Assembly.GetExecutingAssembly().GetName().Version}]";
            logger.LogInformation("Version request: {0}", version);
            return version;
        }
    }
}
