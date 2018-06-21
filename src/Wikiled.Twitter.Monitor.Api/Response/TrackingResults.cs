using System;
using System.Collections.Generic;

namespace Wikiled.Twitter.Monitor.Api.Response
{
    public class TrackingResults
    {
        public TrackingResults()
        {
            Sentiment = new Dictionary<string, SentimentResult>(StringComparer.OrdinalIgnoreCase);
        }

        public string Keyword { get; set; }

        public int Total { get; set; }

        public Dictionary<string, SentimentResult> Sentiment { get; set; }
    }
}
