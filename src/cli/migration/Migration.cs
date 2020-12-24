using CommandDotNet;
using CommandDotNet.Rendering;
using KellermanSoftware.CompareNetObjects;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using yadd.core;

namespace yadd.cli.migration
{
    [Command(Description = "Manages Database Migrations.")]
    public class Migration
    {
        // HACK
        ProviderFactory factory = new ProviderFactory("Host=localhost;Username=giuli;Database=mydb");

        [Command(Description = "Dump user schema info")]
        public void Upgrade(IConsole console, CancellationToken cancellationToken,
            [Required] string baselineHint)
        {
            var repo = Repository.FindUpward();

            var provider = factory.Get();

            var baseline = repo.GetBaseline(baselineHint);
            console.WriteLine($"Baseline {baseline.Id.Filename} matches");

            // baseline matches?
            var schema = provider.DataDefinition.GetInformationSchema();
            var compareLogic = new CompareLogic();
            var compareResult = compareLogic.Compare(schema, baseline.InformationSchema);
            if (!compareResult.AreEqual)
            {
                console.WriteLine("Baseline does not match: cannot apply package");
                return;
            }

            // apply scripts in order
            foreach (var delta in repo.GetDeltas(baseline))
            {
                console.WriteLine($"Applying delta {delta.Id.Filename}");
                foreach (var script in delta.Scripts)
                {
                    console.Write($"Applying script {script.Name}...");
                    (int err, string msg) = provider.ScriptRunner.Run(script.Code);
                    console.WriteLine($"{msg}");
                }
            }
        }
    }
}
