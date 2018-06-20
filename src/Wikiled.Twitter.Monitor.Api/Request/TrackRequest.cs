using System;

namespace Wikiled.Twitter.Monitor.Api.Request
{
    public class TrackRequest
    {
        public string[] Keywords { get; set; }

        public string Domain { get; set; }

        public string Language { get; set; }

        public DateTime? Expire { get; set; }
    }
}
