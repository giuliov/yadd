using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using yadd.core;

namespace yadd.postgresql_provider
{
    public class PostgreSQLProvider : IProvider, IDataDefinition, IScriptRunner
    {
        public string ConnectionString { get; init; }

        public IDataDefinition DataDefinition => this;

        public IScriptRunner ScriptRunner => this;

        public InformationSchema GetInformationSchema()
        {
            var tables = GetInformationSchemaTables();
            var schemata = GetInformationSchemata();
            return new InformationSchema { Schemata = schemata.ToArray(), Tables = tables.ToArray() };
        }

        public (int err, string msg) Run(string scriptCode)
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(scriptCode, conn);
            cmd.ExecuteNonQuery();

            return (0, "OK");
        }

        private IList<InformationSchemata> GetInformationSchemata()
        {
            var schemata = new List<InformationSchemata>();

            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT catalog_name,schema_name,schema_owner FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','pg_toast')", conn);
            using var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (reader.Read())
            {
                /*
                schemata.Add(new InformationSchemata(
                    catalog: reader.GetString(0),
                    schema: reader.GetString(1),
                    owner: reader.GetString(2)
                    ));
                */
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

            using var tablesConn = new NpgsqlConnection(ConnectionString);
            tablesConn.Open();
            using var columnsConn = new NpgsqlConnection(ConnectionString);
            columnsConn.Open();
            using var tablesCmd = new NpgsqlCommand("SELECT table_catalog,table_schema,table_name,table_type FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog','information_schema')", tablesConn);
            using var tablesReader = tablesCmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (tablesReader.Read())
            {
                var columns = new List<InformationSchemaColumn>();

                using var columnsCmd = new NpgsqlCommand($"SELECT column_name,ordinal_position,column_default,is_nullable,data_type,character_maximum_length FROM information_schema.columns WHERE table_catalog ='{ tablesReader.GetString(0) }' AND table_schema= '{tablesReader.GetString(1)}' AND table_name= '{tablesReader.GetString(2)}' ", columnsConn);
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
