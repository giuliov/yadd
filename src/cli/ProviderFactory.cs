namespace yadd.core
{
    public class ProviderFactory
    {
        public IProvider Get(ProviderOptions options)
        {
            // HACK should load provider dynamically, e.g. https://jeremybytes.blogspot.com/2020/01/dynamically-loading-types-in-net-core.html
            return options.ProviderName.ToLower() switch
            {
                "mssql" => new mssql_provider.SQLServerProvider { ConnectionString = options.ConnectionString },
                "postgresql" => new postgresql_provider.PostgreSQLProvider { ConnectionString = options.ConnectionString },
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
