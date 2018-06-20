using System;
using System.Reflection;
using System.Threading.Tasks;
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

        private IStreamMonitor monitor;

        public TwitterController(ILogger<TwitterController> logger, IIpResolve resolve, IStreamMonitor monitor)
        {
            this.resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("result")]
        [HttpPost]
        public Task<TrackingResults> GetResult(string keyword)
        {
            throw new NotImplementedException();
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
