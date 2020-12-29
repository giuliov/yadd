using CommandDotNet;
using CommandDotNet.Rendering;
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

        [Command]
        public void Info(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var provider = factory.Get(options);
            var info = provider.GetServerVersion();
            console.WriteLine(info.Version);
            console.WriteLine(info.FullVersion);
        }

        private static Baseline TakeBaseline(IProvider provider)
        {
            var data = provider.DataDefinition.GetBaselineData();
            var baseline = new Baseline { Data = data, ServerInfo = provider.GetServerVersion() };
            return baseline;
        }

        [Command(Description = "Initialize a repository")]
        public void Init(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var provider = factory.Get(options);
            var baseline = TakeBaseline(provider);
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
            var baseline = TakeBaseline(provider);

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
                        console.WriteLine($"Baseline {baseline.Id.Displayname} on {baseline.Timestamp} from {baseline.ServerInfo.Provider} {baseline.ServerInfo.Version}");
                        return true;
                    },
                    delta =>
                    {
                        console.WriteLine();
                        // apply scripts in order
                        console.WriteLine($"  Delta {delta.Id.Displayname}: {delta.CommitMessage}");
                        foreach (var script in delta.Scripts)
                        {
                            console.WriteLine($"  + Script {script.Name}:");
                            console.WriteLine(script.Code);
                        }
                        return true;
                    });
            }
            console.WriteLine("------------------------------------------------------------");
        }

        [Command(Description = "Searches a baseline matching database state")]
        public void FindBaseline(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var repo = Repository.FindUpward();

            var provider = factory.Get(options);

            var baseline = TakeBaseline(provider);
            var result = repo.FindMatch(baseline);
            console.WriteLine(result.found
                ? $"Found baseline {result.id.Displayname}"
                : $"No match found.");
        }

        [Command(Description = "Migrate database")]
        public void Upgrade(IConsole console, CancellationToken cancellationToken, ProviderOptions options,
            [Option] string fromBaseline,
            [Option] string toBaseline)
        {
            var repo = Repository.FindUpward();

            var provider = factory.Get(options);

            Baseline initialBaseline;
            if (string.IsNullOrEmpty(fromBaseline))
            {
                initialBaseline = repo.GetRootBaseline();
                console.WriteLine($"From: Using root baseline {initialBaseline.Id.Displayname}");
            }
            else
            {
                initialBaseline = repo.GetMatchingBaseline(fromBaseline);
                console.WriteLine($"From: Found {initialBaseline.Id.Displayname} baseline");
            }
            Baseline finalBaseline;
            if (string.IsNullOrEmpty(toBaseline))
            {
                finalBaseline = repo.GetCurrentBaseline();
                console.WriteLine($"To: Using current baseline {finalBaseline.Id.Displayname}");
            }
            else
            {
                finalBaseline = repo.GetMatchingBaseline(toBaseline);
                console.WriteLine($"To: Found {finalBaseline.Id.Displayname} baseline");
            }

            foreach (var item in repo.GetHistoryBetween(initialBaseline, finalBaseline))
            {
                bool ok = item.Match(
                    baseline =>
                    {
                        // baseline matches?
                        console.Write($"Comparing Database with Baseline {baseline.Id.Displayname}");
                        var data = provider.DataDefinition.GetBaselineData();
                        if (data != baseline.Data)
                        {
                            console.WriteLine($": no match, cannot apply changes");
                            return false;
                        }
                        else
                        {
                            console.WriteLine($": matches applying changes");
                            return true;
                        }
                    },
                    delta =>
                    {
                        // apply scripts in order
                        console.WriteLine($"  Applying delta {delta.Id.Displayname}");
                        foreach (var script in delta.Scripts)
                        {
                            console.Write($"  + Applying script {script.Name}...");
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
