using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Xunit;
using yadd.core;
using yadd.provider.mssql;

namespace yadd_mssql_provider.UnitTest
{
    public class ParserTests
    {
        [Fact]
        public void Test1()
        {
            var parser = new SqlServerScriptParser();
            parser.ParseThis(@"
SELECT * FROM tablename WHERE col1 = 42
");
        }

        [Fact]
        public void Test2()
        {
            var parser = new SqlServerScriptParser();

            string source = File.ReadAllText("instawdb.sql");
            parser.ParseThis(source);
        }
    }
}
