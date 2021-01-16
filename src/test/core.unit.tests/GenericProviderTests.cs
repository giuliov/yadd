using FluentAssertions;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace core.unit.tests
{
    public class GenericProviderTests : TestMockDataBase
    {
        [Fact]
        public void Ctor_NoToml_Fails()
        {
            var fsContent = new Dictionary<string, MockFileData> { };
            var fs = new MockFileSystem(fsContent, TestRootDir);

            fs.Invoking(x => new MockDbProvider(fs, "providers.toml"))
                .Should().Throw<System.IO.FileNotFoundException>();
        }

        [Fact]
        public void Ctor_ValidToml_Succeeds()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);

            var provider = new MockDbProvider(fs, "providers.toml");

            provider.ProviderName.Should().Be("mssql");
        }

        [Fact]
        public void IProvider_ProviderVersion_Succeeds()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");

            var result = provider.ProviderVersion;

            result.Should().Be(new Semver.SemVersion(0, 3, 0, "alpha"));
        }

        [Fact]
        public void IProvider_GetServerVersion_Succeeds()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");

            var result = provider.GetServerVersion();
            
            result.Should().Be(TestServerVersionInfo);
        }

        [Fact]
        public void IProvider_ProviderConfigurationData_Succeeds()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");

            var result = provider.ProviderConfigurationData;

            result.Should().Be(TestProviderConfigurationData);
        }

        [Fact]
        public void IDataDefinition_GetBaselineData_Succeeds()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");

            var result = provider.GetBaselineData();

            result.Should().Be(TestSchemaData);
        }

        [Fact]
        public void IScriptRunner_Run_Succeeds()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");

            var result = provider.Run("dummy");

            var expected = (0, "OK");
            result.Should().BeEquivalentTo(expected);
        }

        /*
            protected override IDbCommand NewCommand(string query, IDbConnection connection)
            protected override IDbConnection NewConnection(string connectionString)
        */
    }
}
