using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace yadd.core
{
    public abstract class GenericProvider : IProvider, IDataDefinition, IScriptRunner
    {
        public string ConnectionString { get; init; }

        public IDataDefinition DataDefinition => this;

        public IScriptRunner ScriptRunner => this;

        public virtual SemVersion ProviderVersion => new SemVersion(major: 0, minor: 3, prerelease: "alpha");

        public ServerVersionInfo GetServerVersion()
        {
            using var conn = NewConnection(ConnectionString);
            conn.Open();
            using var cmd = NewCommand(VersionQuery, conn);
            string ver = (string)cmd.ExecuteScalar();
            using var cmd_full = NewCommand(FullVersionQuery, conn);
            string ver_full = (string)cmd_full.ExecuteScalar();
            return new ServerVersionInfo { Provider = ProviderName, Version = ver, FullVersion = ver_full };
        }

        public string GetBaselineData()
        {
            var sb = new StringBuilder();
            using var conn = NewConnection(ConnectionString);
            conn.Open();

            foreach (var query in InformationSchemaQueries)
            {
                AddData(sb, query);
            }

            return sb.ToString();

            void AddData(StringBuilder sb, InformationSchemaQuery query)
            {
                using var cmd = NewCommand(query.SqlQuery, conn);
                using var reader = cmd.ExecuteReader();
                sb.Append(query.Name);
                Serializer.WriteDataReader(sb, reader);
                reader.Close();
            }
        }

        public (int err, string msg) Run(string scriptCode)
        {
            using var conn = NewConnection(ConnectionString);
            conn.Open();
            using var cmd = NewCommand(scriptCode, conn);
            cmd.ExecuteNonQuery();

            return (0, "OK");
        }

        public abstract string ProviderName { get; }
        protected abstract IDbCommand NewCommand(string query, IDbConnection connection);
        protected abstract IDbConnection NewConnection(string connectionString);

        protected string VersionQuery { get; init; }
        protected string FullVersionQuery { get; init; }

        public record InformationSchemaQuery
        {
            public string Name { get; init; }
            public string SqlQuery { get; init; }
        }
        protected IList<InformationSchemaQuery> InformationSchemaQueries { get; init; }

        protected GenericProvider(string configData, string configPath)
        {
            var tomlDoc = Toml.Parse(configData, configPath);
            if (tomlDoc.HasErrors) throw new Exception($"Invalid {configPath} TOML configuration: {tomlDoc.Diagnostics.First()}");
            var tomlTables = tomlDoc.ToModel();
            var table = tomlTables[ProviderName] as TomlTable;
            if (table == null) throw new Exception($"Invalid {configPath} TOML configuration, missing [{ProviderName}]");

            VersionQuery = (string)table["VersionQuery"];
            FullVersionQuery = (string)table["FullVersionQuery"];

            var infoTable = table["infoschema"] as TomlTable;
            if (infoTable == null) throw new Exception($"Invalid {configPath} TOML configuration, missing [{ProviderName}]");

            var sb = new StringBuilder();

            InformationSchemaQueries = new List<InformationSchemaQuery>();
            foreach (var key in infoTable.Keys.OrderBy(k => k))
            {
                var isq = new InformationSchemaQuery { Name = key, SqlQuery = (string)infoTable[key] };
                InformationSchemaQueries.Add(isq);
                sb.Append(isq.SqlQuery);
                sb.Append('\n');
            }

            ProviderConfigurationData = sb.ToString();
        }

        public virtual string ProviderConfigurationData { get; init; }
    }
}