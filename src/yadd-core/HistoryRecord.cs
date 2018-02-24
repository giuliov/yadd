using System;
using System.IO;
using System.Security.Cryptography;

namespace yadd.core
{
    public class HistoryRecord
    {
        public HistoryRecordType RecordType { get; private set; }
        public string Title { get; private set; }
        public string BaseVersion { get; private set; }
        public string Username { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }

        public HistoryRecord(string jobName)
        {
            RecordType = HistoryRecordType.ForwardChange;
            Title = jobName;
            BaseVersion = "";
            Username = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            Timestamp = DateTimeOffset.UtcNow;
        }

        public string GetHash()
        {
            using (var sha = new SHA1Managed())
            {
                using (var stream = File.OpenRead(@"C:\File.ext"))
                {

                    byte[] hashBytes = sha.ComputeHash(stream);
                    string hashString = BitConverter.ToString(hashBytes)
                        .Replace("-", string.Empty);
                }
            }
        }

        public string GetSignature()
        {

        }

        public void TrackSuccess(JobStep jobStep)
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {
            throw new System.NotImplementedException();
        }
    }

    public enum HistoryRecordType
    {
        Baseline,
        ForwardChange,
        Rollback
    }
}