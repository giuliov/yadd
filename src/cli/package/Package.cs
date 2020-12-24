using CommandDotNet;
using CommandDotNet.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using yadd.core;

namespace yadd.cli.package
{
    [Command(Description = "Manages Migration Packages.")]
    public class Package
    {
        // HACK
        ProviderFactory factory = new ProviderFactory("Host=localhost;Username=giuli;Database=mydb");

        [Command(Description = "Initialize package")]
        public void Init(IConsole console, CancellationToken cancellationToken)
        {
            var provider = factory.Get();
            var schema = provider.DataDefinition.GetInformationSchema();

            var baseline = new Baseline { InformationSchema = schema };
            var repo = Repository.Init(baseline);
        }

        const string prefix = "step";

        [Command(Description = "Add script to package")]
        public void Add(IConsole console, CancellationToken cancellationToken,
            [Required] string scriptPath)
        {
            var repo = Repository.FindUpward();
            repo.Stage(scriptPath);
        }

        [Command(Description = "Commits package")]
        public void Commit(IConsole console, CancellationToken cancellationToken,
            [Required] string message)
        {
            var repo = Repository.FindUpward();

            var provider = factory.Get();

            // apply scripts in order
            repo.GetStagedScripts().ToList().ForEach((delta) =>
            {
                console.Write($"Applying script {delta.Name}...");
                (int err, string msg) = provider.ScriptRunner.Run(delta.Code);
                console.WriteLine($"{msg}");
            });

            var schema = provider.DataDefinition.GetInformationSchema();

            var baseline = new Baseline { InformationSchema = schema };
            repo.Commit(message, baseline);
        }
    }
}
