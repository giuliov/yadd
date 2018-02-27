using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace yadd.core
{
    public class DatabaseFactory
    {
        public DatabaseFactory(DbProviderFactory factory, DbConnectionStringBuilder csb, ISchemaExporter exporter)
        {
            Factory = factory;
            Csb = csb;
            Exporter = exporter;
        }

        public DbProviderFactory Factory { get; private set; }
        public DbConnectionStringBuilder Csb { get; private set; }
        public ISchemaExporter Exporter { get; private set; }
    }
}
