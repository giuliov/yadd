using CommandDotNet;
using CommandDotNet.Rendering;
using System.Text.Json;
using System.Threading;
using yadd.core;
using yadd.postgresql_provider;

namespace yadd.cli.schema
{
    [Command(Description = "Manages Database Schema.")]
    public class Schema
    {
        [Command(Description = "Dump user schema info")]
        // TODO Load provider dynamically, e.g. https://jeremybytes.blogspot.com/2020/01/dynamically-loading-types-in-net-core.html
        public void Dump(IConsole console, CancellationToken cancellationToken)
        {
            IProvider provider = new PostgreSQLProvider { ConnectionString = "Host=localhost;Username=giuli;Database=mydb" };
            var schema = provider.DataDefinition.GetInformationSchema();
            string jsonString = JsonSerializer.Serialize(schema);
            console.WriteLine(jsonString);
        }
    }
}
