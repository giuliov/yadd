using System;
using System.Data.Common;
using System.Data.SqlClient;
using Xunit;
using yadd.core;
using yadd.provider.mssql;

namespace yadd_mssql_provider.UnitTest
{
    public class ExporterTests
    {
        [Fact]
        public void Test1()
        {
            var logger = new ConsoleLogger();
            ISchemaExporter exporter = new SqlServerSchemaExporter(logger);
            var csb = new SqlConnectionStringBuilder();
            csb.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FakeVotingPortalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;";
            exporter.ExportSchema(csb, "MyHistoryTable");
        }
    }
}
