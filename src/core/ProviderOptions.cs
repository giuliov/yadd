using CommandDotNet;
using System.ComponentModel.DataAnnotations;

namespace yadd.core
{
    public class ProviderOptions : IArgumentModel
    {
        [EnvVar("YADD_PROVIDERNAME")]
        [Required]
        [Option(Description = "Database Provider name.")]
        public string ProviderName { get; set; }

        [EnvVar("YADD_CONNECTIONSTRING")]
        [Required]
        [Option(Description = "Database Provider connection string.")]
        public string ConnectionString { get; set; }
    }
}
