using System;
using System.Data;

namespace yadd.core
{
    internal class HistoryTable
    {
        public HistoryTable(IDbConnection conn)
        {

        }

        public HistoryRecord AddRecord(string jobName)
        {
            var record = new HistoryRecord(jobName);
            return record;
        }
    }
}
