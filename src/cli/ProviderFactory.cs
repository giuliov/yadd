namespace yadd.core
{
    public class ProviderFactory
    {
        public IProvider Get(ProviderOptions options)
        {
            // HACK
            return options.ProviderName.ToLower() switch
            {
                "mssql" => new mssql_provider.SQLServerProvider { ConnectionString = options.ConnectionString },
                "postgresql" => new postgresql_provider.PostgreSQLProvider { ConnectionString = options.ConnectionString },
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
