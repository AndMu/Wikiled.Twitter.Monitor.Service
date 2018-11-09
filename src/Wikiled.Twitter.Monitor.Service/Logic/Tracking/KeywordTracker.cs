using System;
using Wikiled.Common.Extensions;
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

            Keyword = keyword ?? throw new ArgumentNullException(nameof(keyword));
            IsKeyword = isKeyword;
            RawKeyword = keyword.RemoveBeginingNonLetters();
            Tracker = new Tracker(config);
        }

        public string Keyword { get; }

        public string RawKeyword { get; }

        public bool IsKeyword { get; }

        public ITracker Tracker { get; }
    }
}
