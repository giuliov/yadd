using CommandDotNet;
using CommandDotNet.Rendering;
using KellermanSoftware.CompareNetObjects;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using yadd.core;

namespace yadd.cli
{
    [Command(Description = "Yadd CLI is a tool for managing relational database changes.")]
    public class RootCommand
    {
        ProviderFactory factory = new ProviderFactory();

        [Command(Description = "Initialize a repository")]
        public void Init(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var provider = factory.Get(options);
            var schema = provider.DataDefinition.GetInformationSchema();

            var baseline = new Baseline { InformationSchema = schema };
            var repo = Repository.Init(baseline);
        }

        [Command(Description = "Adds a script to staging area")]
        public void Add(IConsole console, CancellationToken cancellationToken,
            [Required] string scriptPath)
        {
            var repo = Repository.FindUpward();
            repo.Stage(scriptPath);
        }

        [Command(Description = "Removes a script from staging area")]
        public void Remove(IConsole console, CancellationToken cancellationToken,
            [Required] string scriptPath)
        {
            var repo = Repository.FindUpward();
            repo.Unstage(scriptPath);
        }

        [Command(Description = "Lists scripts in staging area")]
        public void ShowStage(IConsole console, CancellationToken cancellationToken)
        {
            var repo = Repository.FindUpward();
            foreach (var item in repo.GetStaged())
            {
                console.WriteLine(item);
            }
        }

        [Command(Description = "Commits staging area")]
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

            // take snapshot of new DB state
            var schema = provider.DataDefinition.GetInformationSchema();
            var baseline = new Baseline { InformationSchema = schema };

            (BaselineId parentId, BaselineId newId) = repo.Commit(message, baseline);

            console.WriteLine($"Committed delta from baseline {parentId.Displayname} to {newId.Displayname}");
        }

        [Command(Description = "Displays repository content")]
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

        [Command(Description = "Migrate database")]
        public void UpgradeFrom(IConsole console, CancellationToken cancellationToken, ProviderOptions options,
            [Required] string fromBaseline)
        {
            var repo = Repository.FindUpward();

            var provider = factory.Get(options);

            var baseline = repo.GetMatchingBaseline(fromBaseline);
            console.WriteLine($"Found {baseline.Id.Displayname} baseline");

            foreach (var item in repo.GetHistorySince(baseline))
            {
                bool ok = item.Match(
                    baseline =>
                    {
                        // baseline matches?
                        var schema = provider.DataDefinition.GetInformationSchema();
                        var compareLogic = new CompareLogic();
                        var compareResult = compareLogic.Compare(schema, baseline.InformationSchema);
                        if (!compareResult.AreEqual)
                        {
                            console.WriteLine($"Database does not match Baseline {baseline.Id.Displayname}: cannot apply package");
                            return false;
                        }
                        else
                        {
                            console.WriteLine($"Database matches Baseline {baseline.Id.Displayname}");
                            return true;
                        }
                    },
                    delta =>
                    {
                        // apply scripts in order
                        console.WriteLine($"Applying delta {delta.Id.Displayname}");
                        foreach (var script in delta.Scripts)
                        {
                            console.Write($"Applying script {script.Name}...");
                            (int err, string msg) = provider.ScriptRunner.Run(script.Code);
                            console.WriteLine($"{msg}");
                        }
                        return true;
                    });
                if (!ok) break;
            }
        }

        [SubCommand]
        public schema.Schema Schema { get; set; }
    }
}
