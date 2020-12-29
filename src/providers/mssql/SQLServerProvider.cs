using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using yadd.core;

namespace yadd.mssql_provider
{
    public class SQLServerQueryProvider : GenericProviderQueriesFromConfig, IGenericProviderQueries
    {
        public override string ProviderName => "mssql";

        public override string VersionQuery { get; protected init; }

        public override string FullVersionQuery { get; protected init; }

        public override string InformationSchemataQuery { get; protected init; }

        public override string InformationSchemaTablesQuery { get; protected init; }

        public override string InformationSchemaColumnsQuery { get; protected init; }

        public override IDbCommand NewCommand(string query, IDbConnection connection)
        {
            return new SqlCommand(query, (SqlConnection)connection);
        }

        public override IDbConnection NewConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }

    public class SQLServerProvider : GenericProvider<SQLServerQueryProvider>
    {
        public SQLServerProvider() : base(new SQLServerQueryProvider()) { }
    }
}
