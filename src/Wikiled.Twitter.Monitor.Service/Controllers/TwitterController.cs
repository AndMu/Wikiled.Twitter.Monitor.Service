using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Server.Core.ActionFilters;
using Wikiled.Server.Core.Controllers;
using Wikiled.Twitter.Monitor.Service.Logic;

namespace Wikiled.Twitter.Monitor.Service.Controllers
{
    [Route("api/[controller]")]
    [TypeFilter(typeof(RequestValidationAttribute))]
    public class TwitterController : BaseController
    {
        private readonly IStreamMonitor monitor;

        public TwitterController(ILoggerFactory loggerFactory, IStreamMonitor monitor) : base(loggerFactory)
        {
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }

        [Route("sentiment/{keyword}")]
        [HttpGet]
        public IActionResult GetResult(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                Logger.LogWarning("Empty keyword");
                return NoContent();
            }

            Logic.Tracking.IKeywordTracker tracker = monitor.Trackers.Resolve(keyword);
            if (tracker == null)
            {
                Logger.LogWarning("Unknown keyword + " + keyword);
                return NotFound("Unknown keyword + " + keyword);
            }

            TrackingResults result = new TrackingResults
            {
                Keyword = keyword
            };
            int[] steps = { 24, 12, 6, 1 };
            foreach (int step in steps)
            {
                result.Sentiment[$"{step}H"] = new TrackingResult { Average = tracker.Tracker.CalculateAverageRating(step), TotalMessages = tracker.Tracker.Count(lastHours: step) };
            }

            result.Total = tracker.Tracker.Count(false);
            return Ok(result);
        }
    }
}
