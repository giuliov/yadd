using System.IO.Abstractions;

namespace yadd.core
{
    public class ProviderFactory
    {
        public IProvider Get(ProviderOptions options)
        {
            var reader = new DefaultProviderDataReader(new FileSystem());
            // HACK should load provider dynamically, e.g. https://jeremybytes.blogspot.com/2020/01/dynamically-loading-types-in-net-core.html
            return options.ProviderName.ToLower() switch
            {
                "mssql" => new mssql_provider.SQLServerProvider(reader.Read(), reader.ConfigurationPath) { ConnectionString = options.ConnectionString },
                "postgresql" => new postgresql_provider.PostgreSQLProvider(reader.Read(), reader.ConfigurationPath) { ConnectionString = options.ConnectionString },
                "mysql" => new mysql_provider.MySQLProvider(reader.Read(), reader.ConfigurationPath) { ConnectionString = options.ConnectionString },
                _ => throw new System.NotImplementedException($"Unknown provider {options.ProviderName}"),
            };
        }
    }
}
