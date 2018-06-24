using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            var tracker = monitor.Trackers.Resolve(keyword);
            if (tracker == null)
            {
                logger.LogWarning("Unknwon keyword + " + keyword);
                return NotFound("Unknwon keyword + " + keyword);
            }

            TrackingResults result = new TrackingResults();
            result.Keyword = keyword;
            int[] steps = { 24, 12, 6, 1 };
            foreach (var step in steps)
            {
                result.Sentiment[$"{step}H"] = new SentimentResult { AverageSentiment = tracker.AverageSentiment(step), TotalMessages = tracker.TotalWithSentiment(step) };
            }

            result.Total = tracker.TotalMessages;
            return Ok(result);
        }

        [Route("version")]
        [HttpGet]
        public string ServerVersion()
        {
            var version = $"Version: [{Assembly.GetExecutingAssembly().GetName().Version}]";
            logger.LogInformation("Version request: {0}", version);
            return version;
        }
    }
}
