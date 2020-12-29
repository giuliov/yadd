using Npgsql;
using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using yadd.core;

namespace yadd.postgresql_provider
{
    public class PostgreSQLQueryProvider : GenericProviderQueriesFromConfig, IGenericProviderQueries
    {
        public override string ProviderName => "postgresql";

        public override string VersionQuery { get; protected init; }

        public override string FullVersionQuery { get; protected init; }

        public override string InformationSchemataQuery { get; protected init; }

        public override string InformationSchemaTablesQuery { get; protected init; }

        public override string InformationSchemaColumnsQuery { get; protected init; }

        public override IDbCommand NewCommand(string query, IDbConnection connection)
        {
            return new NpgsqlCommand(query, (NpgsqlConnection)connection);
        }

        public override IDbConnection NewConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }
    }

    public class PostgreSQLProvider : GenericProvider<PostgreSQLQueryProvider>
    {
        public PostgreSQLProvider() : base(new PostgreSQLQueryProvider()) { }
    }
}
