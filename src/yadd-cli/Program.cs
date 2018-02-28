using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using yadd.core;
using yadd.provider.mssql;

namespace yadd_cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var defaultConfigurationValues = new Dictionary<string, string>
            {
                {"App:Server", @"(localdb)\MSSQLLocalDB"},
                {"App:Database", "FakeVotingPortalDB"},
                {"App:ScriptsFolder", "./TestScripts" }
            };
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                // order => priority
                .AddInMemoryCollection(defaultConfigurationValues)
                .AddEnvironmentVariables()
                .AddIniFile("yadd.ini", optional: true)
                .AddCommandLine(args);
            var config = builder.Build();
            var appConfig = config.GetSection("App").Get<AppSettings>();

            var logger = new ConsoleLogger();

            // target database stuff
            var clientFactory = SqlClientFactory.Instance;
            var csb = clientFactory.CreateConnectionStringBuilder();
            csb.Add("Data Source", appConfig.Server);
            csb.Add("Initial Catalog", appConfig.Database);
            csb.Add("Integrated Security", true);
            var exporter = new SqlServerSchemaExporter(logger);
            var target = new DatabaseFactory(clientFactory, csb, exporter);

            // pick up scripts
            var jobs = new List<Job>();
            foreach (string file in Directory.GetFiles(appConfig.ScriptsFolder))
            {
                jobs.Add(new Job(file));
            }

            var deployer = new Deployer(jobs, target, logger, null);

            var result = deployer.Deploy();

            return result.Errors;
        }
    }
}
