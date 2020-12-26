using Semver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using yadd.core;

namespace yadd.mssql_provider
{
    public class SQLServerQueryProvider : IGenericProviderQueries
    {
        public string ProviderName => "mssql";

        public string VersionQuery => "SELECT SERVERPROPERTY('productversion')";

        public string FullVersionQuery => "SELECT @@version";

        public string InformationSchemataQuery => "SELECT catalog_name,schema_name,schema_owner FROM information_schema.schemata WHERE schema_name NOT IN ('dbo' ,'guest' ,'INFORMATION_SCHEMA' ,'sys' ,'db_owner' ,'db_accessadmin' ,'db_securityadmin' ,'db_ddladmin' ,'db_backupoperator' ,'db_datareader' ,'db_datawriter' ,'db_denydatareader' ,'db_denydatawriter')";

        public string InformationSchemaTablesQuery => "SELECT table_catalog,table_schema,table_name,table_type FROM information_schema.tables";

        public string InformationSchemaColumnsQuery(string catalog, string schema, string table)
            => $"SELECT column_name,ordinal_position,column_default,is_nullable,data_type,character_maximum_length FROM information_schema.columns WHERE table_catalog ='{catalog }' AND table_schema= '{schema}' AND table_name= '{table}' ";

        public IDbCommand NewCommand(string query, IDbConnection connection)
        {
            return new SqlCommand(query, (SqlConnection)connection);
        }

        public IDbConnection NewConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }

    public class SQLServerProvider : GenericProvider<SQLServerQueryProvider>
    {
        public SQLServerProvider() : base(new SQLServerQueryProvider()) { }
    }
}
