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
        private Logger logger;

        public SqlServerSchemaExporter(Logger logger)
        {
            this.logger = logger;
        }

        public string ExportSchema(DbConnectionStringBuilder csb, string historyTableName)
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
            transfer.CopyAllTables = false;
            transfer.CopyAllViews = true;
            // etc
            transfer.Options = options;
            transfer.ScriptingError += Transfer_ScriptingError;
            transfer.ScriptingProgress += Transfer_ScriptingProgress;
            foreach (Table table in database.Tables)
            {
                // TODO: manage schema and owner
                if (table.Name != historyTableName)
                {
                    transfer.ObjectList.Add(table);
                }
            }
            StringCollection scriptBatches = transfer.ScriptTransfer();

            return exportFile;
        }

        private void Transfer_ScriptingError(object sender, ScriptingErrorEventArgs e)
        {
            logger.ErrorWhileExportingTargetDatabaseSchema(e.Current.Value, e.InnerException.Message);
        }

        private void Transfer_ScriptingProgress(object sender, ProgressReportEventArgs e)
        {
            logger.ExportingTargetDatabaseSchemaObject(e.Current.Value);
        }
    }
}
