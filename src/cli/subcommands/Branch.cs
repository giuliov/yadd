using CommandDotNet;
using CommandDotNet.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using yadd.core;

namespace yadd.cli.subcommands
{
    [Command(Description = "Manages Branches.")]
    public class Branch
    {
        [Command(Description = "Lists existing branches")]
        public void List(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var repo = Repository.FindUpward();
            foreach (var item in repo.GetAllBranches())
            {
                console.WriteLine(item.current ? $"{item.name}*" : $"{item.name}");
            }
        }

        [Command(Description = "Switches to a new branch")]
        public void SwitchTo(IConsole console, CancellationToken cancellationToken, ProviderOptions options,
            [Required] BaselineRef newBranch)
        {
            var repo = Repository.FindUpward();
            repo.SwitchTo(newBranch);
        }

        [Command(Description = "Deletes an existing branch")]
        public void Remove(IConsole console, CancellationToken cancellationToken, ProviderOptions options,
            [Required] BaselineRef branch)
        {
            var repo = Repository.FindUpward();
            repo.RemoveBranch(branch);
        }
    }
}