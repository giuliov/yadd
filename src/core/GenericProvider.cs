using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        string InformationSchemaColumnsQuery(string catalog, string schema, string table);
    }

    public class GenericProvider<T> : IProvider, IDataDefinition, IScriptRunner
        where T : IGenericProviderQueries
    {
        T self;
        protected GenericProvider(T child) { self = child; }

        public string ConnectionString { get; init; }

        public IDataDefinition DataDefinition => this;

        public IScriptRunner ScriptRunner => this;

        public virtual SemVersion ProviderVersion => new SemVersion(major: 0, minor: 1, prerelease: "alpha");

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

        public InformationSchema GetInformationSchema()
        {
            var tables = GetInformationSchemaTables();
            var schemata = GetInformationSchemata();
            return new InformationSchema { Schemata = schemata.ToArray(), Tables = tables.ToArray() };
        }

        public (int err, string msg) Run(string scriptCode)
        {
            using var conn = self.NewConnection(ConnectionString);
            conn.Open();
            using var cmd = self.NewCommand(scriptCode, conn);
            cmd.ExecuteNonQuery();

            return (0, "OK");
        }

        private IList<InformationSchemata> GetInformationSchemata()
        {
            var schemata = new List<InformationSchemata>();

            using var conn = self.NewConnection(ConnectionString);
            conn.Open();
            using var cmd = self.NewCommand(self.InformationSchemataQuery, conn);
            using var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (reader.Read())
            {
                schemata.Add(new InformationSchemata
                {
                    Catalog = reader.GetString(0),
                    Schema = reader.GetString(1),
                    Owner = reader.GetString(2)
                });
            }
            reader.Close();

            return schemata;
        }

        private IList<InformationSchemaTable> GetInformationSchemaTables()
        {
            var tables = new List<InformationSchemaTable>();

            using var tablesConn = self.NewConnection(ConnectionString);
            tablesConn.Open();
            using var columnsConn = self.NewConnection(ConnectionString);
            columnsConn.Open();
            using var tablesCmd = self.NewCommand(self.InformationSchemaTablesQuery, tablesConn);
            using var tablesReader = tablesCmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (tablesReader.Read())
            {
                var columns = new List<InformationSchemaColumn>();

                using var columnsCmd = self.NewCommand(self.InformationSchemaColumnsQuery(
                    tablesReader.GetString(0), tablesReader.GetString(1),tablesReader.GetString(2)
                    ), columnsConn);
                using var columnsReader = columnsCmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (columnsReader.Read())
                {
                    columns.Add(new InformationSchemaColumn
                    {
                        Name = columnsReader.GetString(0),
                        Position = columnsReader.GetInt32(1),
                        Default = columnsReader.IsDBNull(2) ? null : columnsReader.GetString(2),
                        Nullable = BoolFromYesOrNoString(columnsReader.GetString(3)),
                        DataType = columnsReader.GetString(4),
                        MaximumLength = columnsReader.IsDBNull(5) ? 0 : columnsReader.GetInt32(5)
                    });
                }
                columnsReader.Close();

                tables.Add(new InformationSchemaTable
                {
                    Catalog = tablesReader.GetString(0),
                    Schema = tablesReader.GetString(1),
                    Name = tablesReader.GetString(2),
                    Type = InformationSchemaTableTypeFromString(tablesReader.GetString(3)),
                    Columns = columns.ToArray()
                });
            }
            tablesReader.Close();

            return tables;

            // local functions

            static bool BoolFromYesOrNoString(string yesOrNo)
            {
                return yesOrNo.ToLower() == "yes";
            }

            static InformationSchemaTable.TableType InformationSchemaTableTypeFromString(string type)
            {
                return type switch
                {
                    "BASE TABLE" => InformationSchemaTable.TableType.BaseTable,
                    "VIEW" => InformationSchemaTable.TableType.View,
                    "FOREIGN" => InformationSchemaTable.TableType.Foreign,
                    "LOCAL TEMPORARY" => InformationSchemaTable.TableType.LocalTemporary,
                    _ => throw new Exception($"Unsupported table type '{type}'")
                };
            }
        }
    }
}