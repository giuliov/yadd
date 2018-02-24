using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace yadd.core
{
    public class DatabaseFactory
    {
        public DatabaseFactory(DbProviderFactory factory, DbConnectionStringBuilder csb)
        {
            Factory = factory;
            Csb = csb;
        }

        public DbProviderFactory Factory { get; private set; }
        public DbConnectionStringBuilder Csb { get; private set; }
    }
}
