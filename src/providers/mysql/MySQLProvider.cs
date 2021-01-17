using MySql.Data.MySqlClient;
using System.Data;
using yadd.core;

namespace yadd.mysql_provider
{
    public class MySQLProvider : GenericProvider
    {
        public override string ProviderName => "mysql";

        public MySQLProvider(string configData, string configPath) : base(configData, configPath) { }
        protected override IDbCommand NewCommand(string query, IDbConnection connection)
        {
            return new MySqlCommand(query, (MySqlConnection)connection);
        }

        protected override IDbConnection NewConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
    }
}
