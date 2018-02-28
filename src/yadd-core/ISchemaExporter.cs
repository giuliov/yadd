using System.Data.Common;
using System.IO;

namespace yadd.core
{
    public interface ISchemaExporter
    {
        string ExportSchema(DbConnectionStringBuilder csb, string historyTableName);
    }
}