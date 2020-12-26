using CommandDotNet;
using CommandDotNet.DataAnnotations;
using CommandDotNet.NameCasing;
using System;

namespace yadd.cli
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return new AppRunner<RootCommand>()
                        .UseDefaultMiddleware(excludePrompting: true)
                        .UseDataAnnotationValidations(showHelpOnError: true)
                        .UseNameCasing(Case.KebabCase)
                        .UseDefaultsFromEnvVar()
                        .Run(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 99;
            }
        }
    }
}
