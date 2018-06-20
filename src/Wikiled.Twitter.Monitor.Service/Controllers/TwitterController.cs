using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Arguments;
using Wikiled.Server.Core.ActionFilters;
using Wikiled.Server.Core.Helpers;
using Wikiled.Twitter.Monitor.Api.Request;

namespace Wikiled.Twitter.Monitor.Service.Controllers
{
    [Route("api/[controller]")]
    [TypeFilter(typeof(RequestValidationAttribute))]
    public class TwitterController : Controller
    {
        private readonly ILogger<TwitterController> logger;

        private readonly IIpResolve resolve;

        public TwitterController(ILogger<TwitterController> logger, IIpResolve resolve)
        {
            Guard.NotNull(() => resolve, resolve);
            this.resolve = resolve;
            this.logger = logger;
        }

        [Route("track")]
        [HttpPost]
        public async Task Track([FromBody]TrackRequest review)
        {
            logger.LogInformation("Track [{0}]", resolve.GetRequestIp());
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
