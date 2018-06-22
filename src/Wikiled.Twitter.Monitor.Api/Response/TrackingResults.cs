using System;
using System.Collections.Generic;
using System.Text;

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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Tracking Result: [{Keyword}]({Total})");
            if (Sentiment != null)
            {
                foreach (var result in Sentiment)
                {
                    builder.Append($" [{result.Key}]:{result.Value}");
                }
            }

            return builder.ToString();
        }
    }
}
