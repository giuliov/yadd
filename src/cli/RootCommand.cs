using CommandDotNet;

namespace yadd.cli
{
    [Command(Description = "Yadd CLI is a tool for managing relation databases.")]
    public class RootCommand
    {
        [SubCommand]
        public schema.Schema Schema { get; set; }
    }
}
