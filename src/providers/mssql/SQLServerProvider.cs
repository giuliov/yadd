using System.Data;
using System.Data.SqlClient;
using yadd.core;

namespace yadd.mssql_provider
{
    public class SQLServerProvider : GenericProvider
    {
        public override string ProviderName => "mssql";

        public SQLServerProvider(string configData, string configPath) : base(configData, configPath) { }

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
