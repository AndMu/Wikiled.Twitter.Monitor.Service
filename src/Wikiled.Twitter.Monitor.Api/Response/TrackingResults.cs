using System.Collections.Generic;

namespace Wikiled.Twitter.Monitor.Api.Response
{
    public class TrackingResults
    {
        public string Keyword { get; set; }

        public Dictionary<string, SentimentResult> Sentiment { get; set; }
    }
}
