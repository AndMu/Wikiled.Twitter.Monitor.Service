using System;
using Wikiled.Common.Utilities.Config;
using Wikiled.MachineLearning.Mathematics.Tracking;

namespace Wikiled.Twitter.Monitor.Service.Logic.Tracking
{
    public class KeywordTracker : IKeywordTracker
    {
        public KeywordTracker(IApplicationConfiguration config, string keyword, bool isKeyword)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Value = keyword ?? throw new ArgumentNullException(nameof(keyword));
            IsKeyword = isKeyword;
            Tracker = new Tracker(config);
        }

        public string Value { get; }

        public bool IsKeyword { get; }

        public ITracker Tracker { get; }
    }
}
