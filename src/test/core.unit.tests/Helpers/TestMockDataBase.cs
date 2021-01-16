using System;
using System.IO.Abstractions.TestingHelpers;
using yadd.core;

namespace core.unit.tests
{
    public class TestMockDataBase
    {
        protected readonly string TestRootDir = @"c:\FAKE";
        protected readonly string TestHash1 = new string('1', 128);
        protected readonly string TestHash2 = new string('2', 128);
        protected readonly string TestScriptFile1 = @"c:\Temp\script01.sql";
        protected readonly string TestScriptFile1Content = "SELECT @@VERSION";
        protected readonly string TestScriptFile1Hash = "61ba912ac17d2eb1f4fdd96c52b382e13079f22af81e4e4a70381536122d1233412fdc778c3e8f40b99c360d37c72d38924a4fad9bde844fc20b9a731726a87a";
        protected readonly string TestScriptFile2 = @"c:\Temp\script02.sql";
        protected readonly string TestScriptFile2Content = "SELECT 1";
        protected readonly string TestScriptFile2Hash = "0b5f5e6a4ebc8633e71906cfcdc28a692b615471fbe8c3a964288cf49237d133582320e8a4f0f5a5706e4472c591d100cb96e0d529a14c260949691ec36dda7c";
        protected readonly string TestBaselineHash = "72b6b42db671c92c6195222803b8d2748cdf312efb9086605d29ca9097cd5c99be4693d172e3db54addbdffa6b3f159ad2684ebcd9ba528155c1c67f651a57f2";
        protected readonly DateTimeOffset TestTimestamp = new DateTimeOffset(2020, 12, 31, 12, 34, 56, 789, new TimeSpan(0, 0, 0, 0));
        protected readonly string TestSchemaData = @"Schemata(catalog_name,schema_name,schema_owner)[
]
TableColumns(table_catalog,table_schema,table_nam,column_name,ordinal_position,column_default,is_nullable,data_type,character_maximum_length)[
]
Tables(table_catalog,table_schema,table_name,table_type)[
]
";
        protected readonly string TestSchemaHash = "d806009e1587f164969147cf8bef3ee29f493841bd613435ccb8a171c8525c9ba1ac0635a709b1d0e37b6747a851b2c17356c70bb99398cbcde6efc8bdfe76e4";
        protected readonly string TestBaselineSerializeName = "72b6b42db671c92c6195222803b8d2748cdf31";
        protected readonly string TestProviderConfigurationData = @"SchemataQuery
TableColumnsQuery
TablesQuery
".Replace("\r", "");
        protected readonly ServerVersionInfo TestServerVersionInfo = new ServerVersionInfo
        {
            Provider = "mssql",
            Version = "15.0",
            FullVersion = "SQL Server 2019"
        };
        protected readonly string TestMeta = @"[meta]
version = ""0.3""
[baseline]
timestamp = ""2020-12-31T12:34:56.7890000+00:00""
schema_hash = ""d806009e1587f164969147cf8bef3ee29f493841bd613435ccb8a171c8525c9ba1ac0635a709b1d0e37b6747a851b2c17356c70bb99398cbcde6efc8bdfe76e4""
provider_configuration_hash = ""de298366fc9250dc440ed1516316bbee5951cda4a68041ce9629468fd9e9164851565799eaacf7fb99dd236fd74975ed3752b6e7929debaeb07cb6b6eea90d6d""
[baseline.serverinfo]
provider = ""mssql""
version = ""15.0""
fullversion = ""SQL Server 2019""
".Replace("\r", "");


        protected MockFileSystem GetEmptyRepo(string repoDir)
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{repoDir}\\staging");
            fs.AddDirectory($"{repoDir}\\baseline");
            fs.AddDirectory($"{repoDir}\\delta");
            fs.AddDirectory($"{repoDir}\\reference\\branches");
            fs.AddDirectory($"{repoDir}\\reference\\tags");
            return fs;
        }


        protected MockFileSystem GetPopulatedRepo() => GetPopulatedRepo(TestRootDir);
        protected MockFileSystem GetPopulatedRepo(string repoDir)
        {
            var fs = GetEmptyRepo(repoDir);
            string baselineDir = $"{repoDir}\\baseline\\{TestBaselineHash.Substring(0, 38)}";
            fs.AddDirectory($"{baselineDir}");
            fs.AddFile($"{baselineDir}\\schema_data", new MockFileData(TestSchemaData));
            fs.AddFile($"{baselineDir}\\schema_hash", new MockFileData(TestSchemaHash));
            fs.AddFile($"{baselineDir}\\meta", new MockFileData(TestMeta));
            fs.AddFile($"{baselineDir}\\provider_configuration", new MockFileData(TestProviderConfigurationData));
            return fs;
        }

        protected MockFileSystem GetFullyPopulatedRepo(string repoDir)
        {
            var fs = GetPopulatedRepo(repoDir);
            fs.AddFile($"{repoDir}\\info", new MockFileData("[yadd]\nversion=\"0.3\""));
            fs.AddFile($"{repoDir}\\reference\\root_baseline", new MockFileData(TestSchemaHash));
            fs.AddFile($"{repoDir}\\reference\\branches\\main", new MockFileData(TestSchemaHash));
            fs.AddFile($"{repoDir}\\currentt_baseline", ":main");
            return fs;
        }
    }
}
