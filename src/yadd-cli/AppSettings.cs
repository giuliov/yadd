using System;

namespace yadd_cli
{
    internal class AppSettings
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ScriptsFolder { get; set; }
        public string OutputFile { get; set; } = "yadd.sql";

        internal bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Server) && !string.IsNullOrWhiteSpace(Database);
        }

        internal void ShowHelp()
        {
            Console.WriteLine($@"
Usage: yadd --server <sqlserver_instance> --database <database> [--username <user> --password <password>] [--scriptsFolder <folder>] [--outputFile <file>]
");
        }
    }
}