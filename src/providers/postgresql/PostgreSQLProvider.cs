using Npgsql;
using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using yadd.core;

namespace yadd.postgresql_provider
{
    public class PostgreSQLProvider : GenericProvider
    {
        public override string ProviderName => "postgresql";

        protected override IDbCommand NewCommand(string query, IDbConnection connection)
        {
            return new NpgsqlCommand(query, (NpgsqlConnection)connection);
        }

        protected override IDbConnection NewConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}
