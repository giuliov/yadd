using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using yadd.core;

namespace yadd.mssql_provider
{
    public class SQLServerProvider : GenericProvider
    {
        public override string ProviderName => "mssql";

        protected override IDbCommand NewCommand(string query, IDbConnection connection)
        {
            return new SqlCommand(query, (SqlConnection)connection);
        }

        protected override IDbConnection NewConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
