using Npgsql;
using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using yadd.core;

namespace yadd.postgresql_provider
{
    public class PostgreSQLQueryProvider : IGenericProviderQueries
    {
        public string ProviderName => "postgresql";

        public string VersionQuery => "SHOW server_version";

        public string FullVersionQuery => "SELECT version()";

        public string InformationSchemataQuery => "SELECT catalog_name,schema_name,schema_owner FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','pg_toast')";

        public string InformationSchemaTablesQuery => "SELECT table_catalog,table_schema,table_name,table_type FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog','information_schema')";

        public string InformationSchemaColumnsQuery(string catalog, string schema, string table)
            => $"SELECT column_name,ordinal_position,column_default,is_nullable,data_type,character_maximum_length FROM information_schema.columns WHERE table_catalog ='{catalog }' AND table_schema= '{schema}' AND table_name= '{table}' ";

        public IDbCommand NewCommand(string query, IDbConnection connection)
        {
            return new NpgsqlCommand(query, (NpgsqlConnection)connection);
        }

        public IDbConnection NewConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }
    }

    public class PostgreSQLProvider : GenericProvider<PostgreSQLQueryProvider>
    {
        public PostgreSQLProvider() : base(new PostgreSQLQueryProvider()) { }
    }
}
