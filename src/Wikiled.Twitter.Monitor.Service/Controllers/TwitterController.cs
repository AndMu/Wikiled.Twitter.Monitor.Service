﻿using Microsoft.AspNetCore.Mvc;
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

            var tracker = manager.Resolve(keyword, "Keyword");
            TrackingResults result = new TrackingResults
            {
                Keyword = keyword
            };
            int[] steps = { 24, 12, 6, 1 };
            foreach (int step in steps)
            {
                result.Sentiment[$"{step}H"] = new TrackingResult { Average = tracker.CalculateAverageRating(step), TotalMessages = tracker.Count(lastHours: step) };
            }

            result.Total = tracker.Count(false);
            return Ok(result);
        }
    }
}
