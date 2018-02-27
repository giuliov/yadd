using System;
using System.Data;
using System.IO;
using System.Text;

namespace yadd.core
{
    internal class HistoryTable
    {
        public string TableName => "YaddHistory";
        public HashValue BaselineVersion { get; private set; }
        public string Username { get; private set; }

        public HistoryTable(string dbSchemaExportPath)
        {
            string dbSchemaExport = File.ReadAllText(dbSchemaExportPath, Encoding.UTF8);
            BaselineVersion = new HashValue(dbSchemaExport);
            Username = Environment.UserName; // TODO consider System.Threading.Thread.CurrentPrincipal.Identity.Name;
        }

        public HistoryRecord AddRecord(Job job)
        {
            var record = new HistoryRecord(job, BaselineVersion);
            return record;
        }
    }
}
