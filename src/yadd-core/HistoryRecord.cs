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
        public HashValue BaseVersion { get; private set; }
        public HashValue ScriptVersion { get; private set; }
        public string Username { get; private set; }
        public DateTimeOffset StartDate { get; private set; }
        public DateTimeOffset? FinishDate { get; private set; }
        public string Description { get; internal set; }

        public HistoryRecord(Job job, HashValue baselineVersion)
        {
            RecordType = HistoryRecordType.ForwardChange;
            Title = job.Name;
            ScriptVersion = job.GetHash();
            BaseVersion = baselineVersion;
            Username = Environment.UserName; // TODO consider System.Threading.Thread.CurrentPrincipal.Identity.Name;
            StartDate = DateTimeOffset.UtcNow;
            Description = string.Empty; // TODO
        }

        public string TextualRepresentation
        {
            get
            {
                // WARNING: do not ever change this!
                return $@"RecordType={RecordType}
Title={Title}
BaseVersion={BaseVersion}
ScriptVersion={ScriptVersion}
Username={Username}
StartDate={StartDate:o}
<<<";
            }
        }

        public HashValue GetHash()
        {
            return new HashValue(TextualRepresentation);
        }

        public void TrackSuccess(JobStep jobStep)
        {
            // TODO no-op for now
        }

        public void Close()
        {
            FinishDate = DateTimeOffset.UtcNow;
        }
    }

    public enum HistoryRecordType
    {
        Baseline        = 'B',
        ForwardChange   = 'F',
        Rollback        = 'R'
    }
}