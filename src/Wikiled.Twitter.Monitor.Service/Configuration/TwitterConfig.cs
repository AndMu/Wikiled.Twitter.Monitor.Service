﻿namespace Wikiled.Twitter.Monitor.Service.Configuration
{
    public class TwitterConfig
    {
        public string Persistency { get; set; }

        public bool HashKeywords { get; set; }

        public string[] Keywords { get; set; }

        public string[] Users { get; set; }

        public string[] Languages { get; set; }
    }
}
