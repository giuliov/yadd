using yadd.postgresql_provider;

namespace yadd.core
{
    public class ProviderFactory
    {
        string ConnectionString { get; init; }

        public ProviderFactory(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IProvider Get()
        {
            return new PostgreSQLProvider { ConnectionString = this.ConnectionString };
        }
    }
}
