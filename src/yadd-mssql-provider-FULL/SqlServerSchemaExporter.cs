using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using yadd.core;
using System.Linq;

namespace yadd.provider.mssql
{
    public class SqlServerSchemaExporter : ISchemaExporter
    {
        public string ExportSchema(DbConnectionStringBuilder csb)
        {
            string exportFile = Path.ChangeExtension(Path.GetTempFileName(), ".sql");

            var builder = csb as SqlConnectionStringBuilder;
            var server = new Server(builder.DataSource);
            var database = server.Databases[builder.InitialCatalog];
            var options = new ScriptingOptions()
            {
                Encoding = Encoding.UTF8,
                SchemaQualify = true,
                ToFileOnly = true,
                FileName = exportFile
            };

            var transfer = new Transfer(database);
            transfer.CopyAllObjects = false;
            transfer.CopyAllTables = true;
            transfer.CopyAllViews = true;
            transfer.Options = options;
            //transfer.Options.TargetServerVersion = SqlServerVersion.Version90;
            StringCollection scriptBatches = transfer.ScriptTransfer();

            return exportFile;
        }
    }
}
