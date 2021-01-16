using CommandDotNet;
using CommandDotNet.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using yadd.core;

namespace yadd.cli.subcommands
{
    [Command(Description = "Manages Tags.")]
    public class Tag
    {
        [Command(Description = "Lists existing tags")]
        public void List(IConsole console, CancellationToken cancellationToken, ProviderOptions options)
        {
            var repo = Repository.FindUpward();
            foreach (var item in repo.GetAllTags())
            {
                console.WriteLine($"{item.tag}: {item.id.Displayname}");
            }
        }

        [Command(Description = "Adds a tag to a baseline")]
        public void Add(IConsole console, CancellationToken cancellationToken, ProviderOptions options,
            [Required] BaselineRef tag, [Required] BaselineRef target)
        {
            var repo = Repository.FindUpward();
            repo.AddTag(tag, target);
        }

        [Command(Description = "Removes a tag")]
        public void Remove(IConsole console, CancellationToken cancellationToken, ProviderOptions options,
            [Required] BaselineRef tag)
        {
            var repo = Repository.FindUpward();
            repo.RemoveTag(tag);
        }
    }
}