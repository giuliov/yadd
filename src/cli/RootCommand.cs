using CommandDotNet;

namespace yadd.cli
{
    [Command(Description = "Yadd CLI is a tool for managing relation databases.")]
    public class RootCommand
    {
        [SubCommand]
        public package.Package Package { get; set; }
        [SubCommand]
        public schema.Schema Schema { get; set; }

        [SubCommand]
        public migration.Migration Migration { get; set; }
    }
}
