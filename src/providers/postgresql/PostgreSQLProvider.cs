using Npgsql;
using System.Data;
using yadd.core;

namespace yadd.postgresql_provider
{
    public class PostgreSQLProvider : GenericProvider
    {
        public override string ProviderName => "postgresql";

        public PostgreSQLProvider(string configData, string configPath) : base(configData, configPath) { }

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
