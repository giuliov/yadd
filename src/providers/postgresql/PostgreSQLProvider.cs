using Npgsql;
using System.Collections.Generic;
using System.Data;
using yadd.core;

namespace yadd.postgresql_provider
{
    public class PostgreSQLProvider : IProvider, IDataDefinition
    {
        public IDataDefinition DataDefinition => this;

        public InformationSchema GetInformationSchema()
        {
            var tables = GetInformationSchemaTables();
            var schemata = GetInformationSchemata();
            return new PSqlInformationSchema(schemata: schemata, tables: tables);
        }

        private IList<InformationSchemata> GetInformationSchemata()
        {
            var schemata = new List<InformationSchemata>();

            using var conn = new NpgsqlConnection("Host=localhost;Username=giuli;Database=mydb");
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT catalog_name,schema_name,schema_owner FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','pg_toast')", conn);
            using var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (reader.Read())
            {
                schemata.Add(new InformationSchemata(
                    catalog: reader.GetString(0),
                    schema: reader.GetString(1),
                    owner: reader.GetString(2)
                    ));
            }
            reader.Close();

            return schemata;
        }

        private IList<InformationSchemaTable> GetInformationSchemaTables()
        {
            var tables = new List<InformationSchemaTable>();

            using var tablesConn = new NpgsqlConnection("Host=localhost;Username=giuli;Database=mydb");
            tablesConn.Open();
            using var columnsConn = new NpgsqlConnection("Host=localhost;Username=giuli;Database=mydb");
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
                    columns.Add(new InformationSchemaColumn(
                        name: columnsReader.GetString(0),
                        position: columnsReader.GetInt32(1),
                        @default: columnsReader.IsDBNull(2) ? null : columnsReader.GetString(2),
                        nullable: columnsReader.GetString(3),
                        dataType: columnsReader.GetString(4),
                        maximumLength: columnsReader.GetInt32(5)
                        ));
                }
                columnsReader.Close();

                tables.Add(new InformationSchemaTable(
                    catalog: tablesReader.GetString(0),
                    schema: tablesReader.GetString(1),
                    name: tablesReader.GetString(2),
                    type: tablesReader.GetString(3),
                    columns: columns
                    ));
            }
            tablesReader.Close();

            return tables;
        }
    }
}
