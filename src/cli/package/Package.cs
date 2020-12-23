using CommandDotNet;
using CommandDotNet.Rendering;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using yadd.cli.schema;
using yadd.core;
using yadd.postgresql_provider;

namespace yadd.cli.package
{
    [Command(Description = "Manages Migration Packages.")]
    public class Package
    {
        [Command(Description = "Initialize package")]
        public void Init(IConsole console, CancellationToken cancellationToken)
        {
            IProvider provider = new PostgreSQLProvider { ConnectionString = "Host=localhost;Username=giuli;Database=mydb" };
            var schema = provider.DataDefinition.GetInformationSchema();

            string path = Repository.Init();

            string jsonString = JsonSerializer.Serialize(schema);

            File.WriteAllText(Path.Combine(path, "schema.json"), jsonString);
            string hash = Hasher.GetHash(jsonString);
            File.WriteAllText(Path.Combine(path, "schema.hash"), hash);

        }

        const string prefix = "step";

        [Command(Description = "Add script to package")]
        public void Add(IConsole console, CancellationToken cancellationToken,
            string scriptPath)
        {
            string repo = Repository.FindRepo();
            var steps = Directory.EnumerateDirectories(repo, $"{prefix}*");
            var lastStepName = steps.LastOrDefault();
            int lastStep = lastStepName != null ? int.Parse(Path.GetFileName(lastStepName).Substring(prefix.Length)) : 0;
            string thisStep = $"{prefix}{lastStep + 1 }";

            Directory.CreateDirectory(Path.Combine(repo, thisStep));

            File.Copy(scriptPath, Path.Combine(repo, thisStep, "script.sql"));
            string scriptHash = Hasher.GetHash(File.ReadAllBytes(scriptPath));
            File.WriteAllText(Path.Combine(repo, thisStep, "script.hash"), scriptHash);
            File.Copy(Path.Combine(repo, "baseline", "schema.hash"), Path.Combine(repo, thisStep, "parent.hash"));
        }

        [Command(Description = "Commits package")]
        public void Commit(IConsole console, CancellationToken cancellationToken,
            string message)
        {
            string repo = Repository.FindRepo();
            File.WriteAllText(Path.Combine(repo, "commit_message.txt"), message);
        }
    }
}
