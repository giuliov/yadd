using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace yadd.core
{
    public interface IGenericProviderQueries
    {
        IDbConnection NewConnection(string connectionString);
        IDbCommand NewCommand(string query, IDbConnection connection);

        string ProviderName { get; }
        string VersionQuery { get; }
        string FullVersionQuery { get; }
        string InformationSchemataQuery { get; }
        string InformationSchemaTablesQuery { get; }
        string InformationSchemaColumnsQuery { get; }
    }

    public abstract class GenericProviderQueriesFromConfig : IGenericProviderQueries
    {
        public abstract string ProviderName { get; }
        public abstract string VersionQuery { get; protected init; }
        public abstract string FullVersionQuery { get; protected init; }
        public abstract string InformationSchemataQuery { get; protected init; }
        public abstract string InformationSchemaTablesQuery { get; protected init; }

        public abstract string InformationSchemaColumnsQuery { get; protected init; }
        public abstract IDbCommand NewCommand(string query, IDbConnection connection);
        public abstract IDbConnection NewConnection(string connectionString);

        public GenericProviderQueriesFromConfig(string configPath = "providers.toml")
        {
            string tomlString = File.ReadAllText(configPath);
            var tomlDoc = Toml.Parse(tomlString, configPath);
            if (tomlDoc.HasErrors) throw new Exception($"Invalid {configPath} TOML configuration: {tomlDoc.Diagnostics.First()}");
            var tomlTables = tomlDoc.ToModel();
            var table = tomlTables[ProviderName] as TomlTable;
            if (table==null) throw new Exception($"Invalid {configPath} TOML configuration, missing [{ProviderName}]");
            VersionQuery = (string)table["VersionQuery"];
            FullVersionQuery = (string)table["FullVersionQuery"];
            InformationSchemataQuery = (string)table["InformationSchemataQuery"];
            InformationSchemaTablesQuery = (string)table["InformationSchemaTablesQuery"];
            InformationSchemaColumnsQuery = (string)table["InformationSchemaColumnsQuery"];
        }
    }

    public class GenericProvider<T> : IProvider, IDataDefinition, IScriptRunner
        where T : IGenericProviderQueries
    {
        T self;
        protected GenericProvider(T child) { self = child; }

        public string ConnectionString { get; init; }

        public IDataDefinition DataDefinition => this;

        public IScriptRunner ScriptRunner => this;

        public virtual SemVersion ProviderVersion => new SemVersion(major: 0, minor: 3, prerelease: "alpha");

        public ServerVersionInfo GetServerVersion()
        {
            using var conn = self.NewConnection(ConnectionString);
            conn.Open();
            using var cmd = self.NewCommand(self.VersionQuery, conn);
            string ver = (string)cmd.ExecuteScalar();
            using var cmd_full = self.NewCommand(self.FullVersionQuery, conn);
            string ver_full = (string)cmd_full.ExecuteScalar();
            return new ServerVersionInfo { Provider = self.ProviderName, Version = ver, FullVersion = ver_full };
        }

        public string GetBaselineData()
        {
            var sb = new StringBuilder();
            using var conn = self.NewConnection(ConnectionString);
            conn.Open();

            AddData(sb, self.InformationSchemataQuery, "InformationSchemata");
            AddData(sb, self.InformationSchemaTablesQuery, "InformationSchemaTables");
            AddData(sb, self.InformationSchemaColumnsQuery, "InformationSchemaColumns");

            void AddData(StringBuilder sb, string query, string name)
            {
                using var cmd = self.NewCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                sb.Append(name);
                Serializer.WriteDataReader(sb, reader);
                reader.Close();
            }

            return sb.ToString();
        }

        public (int err, string msg) Run(string scriptCode)
        {
            using var conn = self.NewConnection(ConnectionString);
            conn.Open();
            using var cmd = self.NewCommand(scriptCode, conn);
            cmd.ExecuteNonQuery();

            return (0, "OK");
        }
    }
}