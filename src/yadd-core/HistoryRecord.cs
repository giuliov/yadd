using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace yadd.core
{
    public class HistoryRecord
    {
        public HistoryRecordType RecordType { get; private set; }
        public string Title { get; private set; }
        public byte[] BaseVersion { get; private set; }
        public string Username { get; private set; }
        public DateTimeOffset StartDate { get; private set; }
        public DateTimeOffset? FinishDate { get; private set; }

        public HistoryRecord(string jobName)
        {
            RecordType = HistoryRecordType.ForwardChange;
            Title = jobName;
            BaseVersion = new byte[0];
            Username = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            StartDate = DateTimeOffset.UtcNow;
        }

        private string GetTextualRepresentation()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[RecordType]='{0}',", RecordType);
            sb.AppendFormat("[Title]='{0}',", Title);
            sb.AppendFormat("[Username]='{0}',", Username);
            sb.AppendFormat("[StartDate]='{0:o}',", StartDate);
            sb.AppendFormat("[BaseVersion]={0},", BaseVersion.ToHexString());
            sb.AppendFormat("[FinishDate]='{0:o}',", FinishDate);
            sb.AppendFormat("[Description]='{0}',", string.Empty);
            return sb.ToString();
        }

        byte[] GetHash()
        {
            string text = GetTextualRepresentation();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var sha = new SHA1Managed())
            {
                byte[] hashBytes = sha.ComputeHash(bytes);
                return hashBytes;
            }
        }

        public string GetSignature()
        {
            throw new System.NotImplementedException();
            return null;
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