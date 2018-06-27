using System.IO;
using System.Text;
using CsvHelper;
using Tweetinvi.Models;
using Wikiled.Twitter.Persistency;

namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public class TwitPersistency
    {
        private readonly IStreamSource streamSource;

        private readonly object syncRoot = new object();

        public TwitPersistency(IStreamSource streamSource)
        {
            this.streamSource = streamSource;
        }

        public void Save(ITweet message, double? sentiment)
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
                    csvDataTarget.WriteField(message.RetweetedTweet?.Id);
                    csvDataTarget.WriteField(message.RetweetedTweet?.CreatedBy.Id);
                    csvDataTarget.WriteField(message.QuotedTweet?.Id);
                    csvDataTarget.WriteField(message.QuotedTweet?.CreatedBy.Id);
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
