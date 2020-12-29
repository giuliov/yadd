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
        ProviderFactory factory = new ProviderFactory();

        [Command(Description = "Dump user schema info")]
        public void Dump(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var provider = factory.Get(options);
            string data = provider.DataDefinition.GetBaselineData();
            console.WriteLine(data);
        }
    }
}
