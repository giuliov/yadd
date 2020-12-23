using CommandDotNet;
using CommandDotNet.DataAnnotations;
using CommandDotNet.NameCasing;

namespace yadd.cli
{
    class Program
    {
        static int Main(string[] args)
        {
            return new AppRunner<RootCommand>()
                    .UseDefaultMiddleware(excludePrompting: true)
                    .UseDataAnnotationValidations(showHelpOnError: true)
                    .UseNameCasing(Case.KebabCase)
                    .UseDefaultsFromEnvVar()
                    .Run(args);
        }
    }
}
