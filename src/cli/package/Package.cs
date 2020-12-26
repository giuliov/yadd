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
        ProviderFactory factory = new ProviderFactory();

        [Command(Description = "Initialize package")]
        public void Init(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var provider = factory.Get(options);
            var schema = provider.DataDefinition.GetInformationSchema();

            var baseline = new Baseline { InformationSchema = schema };
            var repo = Repository.Init(baseline);
        }

        [Command(Description = "Add script to staging area")]
        public void Add(IConsole console, CancellationToken cancellationToken,
            [Required] string scriptPath)
        {
            var repo = Repository.FindUpward();
            repo.Stage(scriptPath);
        }

        [Command(Description = "Remove script from staging area")]
        public void Remove(IConsole console, CancellationToken cancellationToken,
            [Required] string scriptPath)
        {
            var repo = Repository.FindUpward();
            repo.Unstage(scriptPath);
        }

        [Command(Description = "List script in staging area")]
        public void ShowStage(IConsole console, CancellationToken cancellationToken)
        {
            var repo = Repository.FindUpward();
            foreach (var item in repo.GetStaged())
            {
                console.WriteLine(item);
            }
        }

        [Command(Description = "Commits staging area to package")]
        public void Commit(IConsole console, CancellationToken cancellationToken, ProviderOptions options,
            [Required] string message)
        {
            var repo = Repository.FindUpward();

            var provider = factory.Get(options);

            // apply scripts in order
            repo.GetStagedScripts().ToList().ForEach((delta) =>
            {
                console.Write($"Applying script {delta.Name}...");
                (int err, string msg) = provider.ScriptRunner.Run(delta.Code);
                console.WriteLine($"{msg}");
            });

            var schema = provider.DataDefinition.GetInformationSchema();
            var baseline = new Baseline { InformationSchema = schema };

            (BaselineId parentId, BaselineId newId) = repo.Commit(message, baseline);

            console.WriteLine($"Committed delta from baseline {parentId.Displayname} to {newId.Displayname}");
        }

        [Command(Description = "Display a package content")]
        public void History(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var repo = Repository.FindUpward();
            var provider = factory.Get(options);

            foreach (var item in repo.GetFullHistory())
            {
                item.Match(
                    baseline =>
                    {
                        console.WriteLine("------------------------------------------------------------");
                        console.WriteLine($"Baseline {baseline.Id.Displayname} @ {baseline.Timestamp}");
                        return true;
                    },
                    delta =>
                    {
                        // apply scripts in order
                        console.WriteLine($"  Delta {delta.Id.Displayname}: {delta.CommitMessage}");
                        foreach (var script in delta.Scripts)
                        {
                            console.WriteLine($"  Script {script.Name}:");
                            console.WriteLine(script.Code);
                        }
                        return true;
                    });
            }
            console.WriteLine("------------------------------------------------------------");
        }
    }
}
