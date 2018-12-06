using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Server.Core.ActionFilters;
using Wikiled.Server.Core.Controllers;
using Wikiled.Twitter.Monitor.Service.Requests;

namespace Wikiled.Twitter.Monitor.Service.Controllers
{
    [Route("api/[controller]")]
    [TypeFilter(typeof(RequestValidationAttribute))]
    public class TwitterController : BaseController
    {
        private readonly ITrackingManager manager;

        public TwitterController(ILoggerFactory loggerFactory, ITrackingManager manager) : base(loggerFactory)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
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

            return Ok(RequestSingle(keyword));
        }

        [HttpPost("sentimentAll")]
        public IActionResult GetResultAll(string[] keywords)
        {
            Dictionary<string, TrackingResults> result = new Dictionary<string, TrackingResults>();
            foreach (var keyword in keywords)
            {
                result[keyword] = RequestSingle(keyword);
            }

            return Ok(result);
        }

        [Route("history/{hours}/{keyword}")]
        [HttpGet]
        public IActionResult GetResultHistory(HistoryRequest request)
        {
            if (string.IsNullOrEmpty(request.Keyword))
            {
                Logger.LogWarning("Empty keyword");
                return NoContent();
            }

            return Ok(GetSingleHistory(request));
        }

        private RatingRecord[] GetSingleHistory(HistoryRequest request)
        {
            var tracker = manager.Resolve(request.Keyword, "Keyword");
            return tracker.GetRatings(request.Hours).OrderByDescending(item => item.Date).ToArray();
        }

        [HttpPost("historyall")]
        public IActionResult GetResultHistoryAll(HistoryRequest[] request)
        {
            

            var tracker = manager.Resolve(request.Keyword, "Keyword");
            return Ok(tracker.GetRatings(request.Hours).OrderByDescending(item => item.Date));
        }

        private TrackingResults RequestSingle(string keyword)
        {
            var tracker = manager.Resolve(keyword, "Keyword");
            TrackingResults result = new TrackingResults { Keyword = keyword };

            int[] steps = { 24, 12, 6, 1 };
            foreach (int step in steps)
            {
                result.Sentiment[$"{step}H"] = new TrackingResult
                {
                    Average = tracker.CalculateAverageRating(step),
                    TotalMessages = tracker.Count(lastHours: step)
                };
            }

            result.Total = tracker.Count(false);
            return result;
        }
    }
}
