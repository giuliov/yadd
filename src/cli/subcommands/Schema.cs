using CommandDotNet;
using CommandDotNet.Rendering;
using System.Threading;
using yadd.core;

namespace yadd.cli.subcommands
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
