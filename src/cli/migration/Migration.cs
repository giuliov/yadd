using CommandDotNet;
using CommandDotNet.Rendering;
using System.Threading;

namespace yadd.cli.migration
{
    [Command(Description = "Manages Database Migrations.")]
    public class Migration
    {
        [Command(Description = "Dump user schema info")]
        public void Upgrade(IConsole console, CancellationToken cancellationToken)
        {
            // TODO pick a package and deploy against a target
        }
    }
}
