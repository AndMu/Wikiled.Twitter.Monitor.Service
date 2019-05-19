using System;
using System.IO;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;
using Wikiled.Twitter.Persistency;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class TwitPersistency : ITwitPersistency
    {
        private readonly IStreamSource streamSource;

        private readonly object syncRoot = new object();

        private readonly ILogger<TwitPersistency> logger;

        public TwitPersistency(ILogger<TwitPersistency> logger, IStreamSource streamSource)
        {
            this.streamSource = streamSource ?? throw new System.ArgumentNullException(nameof(streamSource));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Save(ITweet message, double? sentiment)
        {
            if (message == null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }

            logger.LogDebug("Saving message: {0}", message.Id);
            try
            {
                SaveInternal(message, sentiment);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Persistency");
                lock (syncRoot)
                {
                    streamSource.Reset();
                }

                throw;
            }
        }

        private void SaveInternal(ITweet message, double? sentiment)
        {
            var text = message.Text.Replace("\r\n", " ").Replace("\n", " ");
            lock (syncRoot)
            {
                var stream = streamSource.GetStream();
                using (var streamOut = new StreamWriter(stream, Encoding.UTF8, 4096, true))
                using (var csvDataTarget = new CsvWriter(streamOut))
                {
                    csvDataTarget.Configuration.Delimiter = "\t";
                    csvDataTarget.WriteField(message.CreatedAt);
                    csvDataTarget.WriteField(message.Id);
                    csvDataTarget.WriteField(message.CreatedBy.Id);
                    string type = "Message";
                    if (message.RetweetedTweet != null)
                    {
                        csvDataTarget.WriteField(message.RetweetedTweet.Id);
                        csvDataTarget.WriteField(message.RetweetedTweet.CreatedBy.Id);
                        type = "Retweet";
                    }
                    else
                    {
                        csvDataTarget.WriteField(message.QuotedTweet?.Id);
                        csvDataTarget.WriteField(message.QuotedTweet?.CreatedBy.Id);
                        if (message.QuotedTweet != null)
                        {
                            type = "Quote";
                        }
                    }

                    csvDataTarget.WriteField(type);
                    csvDataTarget.WriteField(message.Language);
                    csvDataTarget.WriteField(sentiment);
                    csvDataTarget.WriteField(text);
                    csvDataTarget.NextRecord();
                    streamOut.Flush();
                }

                stream.Flush();
            }
        }
    }
}
