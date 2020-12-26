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
        ProviderFactory factory = new ProviderFactory();

        [Command(Description = "Migrate database")]
        public void Upgrade(IConsole console, CancellationToken cancellationToken, ProviderOptions options,
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
    }
}
