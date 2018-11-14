using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Wikiled.Server.Core.ActionFilters;
using Wikiled.Server.Core.Controllers;
using Wikiled.Twitter.Monitor.Api.Response;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Service.Controllers
{
    [Route("api/[controller]")]
    [TypeFilter(typeof(RequestValidationAttribute))]
    public class TwitterController : BaseController
    {
        private readonly ILogger<TwitterController> logger;

        private readonly IStreamMonitor monitor;

        public TwitterController(ILoggerFactory loggerFactory, IStreamMonitor monitor) : base(loggerFactory)
        {
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            logger = loggerFactory?.CreateLogger<TwitterController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("sentiment/{keyword}")]
        [HttpGet]
        public IActionResult GetResult(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                logger.LogWarning("Empty keyword");
                return NoContent();
            }

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
    }
}
