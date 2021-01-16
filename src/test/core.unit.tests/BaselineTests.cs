using FluentAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using yadd.core;

namespace core.unit.tests
{
    public class BaselineTests : TestMockDataBase
    {
        [Fact]
        public void Serialize_ValidBaseline_Succeeds()
        {
            // Arrange
            var fs = new MockFileSystem();
            var baseline = new Baseline
            {
                Id = new BaselineId(TestBaselineHash),
                Data = TestSchemaData,
                Timestamp = TestTimestamp,
                ProviderConfigurationData = TestProviderConfigurationData,
                ServerInfo = TestServerVersionInfo
            };
            // Act
            var id = baseline.SerializeTo(TestRootDir, fs);
            // Assert
            id.Should().NotBeNull();
            string baselineDir = $"{TestRootDir}\\{TestBaselineSerializeName}";
            fs.FileExists($"{baselineDir}\\schema_data").Should().BeTrue();
            fs.GetFile($"{baselineDir}\\schema_data").TextContents
                .Should().Be(TestSchemaData);
            fs.FileExists($"{baselineDir}\\schema_hash").Should().BeTrue();
            fs.GetFile($"{baselineDir}\\schema_hash").TextContents
                .Should().Be(TestSchemaHash);
            fs.FileExists($"{baselineDir}\\meta").Should().BeTrue();
            fs.GetFile($"{baselineDir}\\meta").TextContents
                .Should().Be(TestMeta);
        }

        [Fact]
        public void Deserialize_ValidBaseline_Succeeds()
        {
            // Arrange
            var fs = new MockFileSystem();
            fs.AddFile($"{TestRootDir}\\schema_data", new MockFileData(TestSchemaData));
            fs.AddFile($"{TestRootDir}\\schema_hash", new MockFileData(TestSchemaHash));
            fs.AddFile($"{TestRootDir}\\meta", new MockFileData(TestMeta));
            fs.AddFile($"{TestRootDir}\\provider_configuration", new MockFileData(TestProviderConfigurationData));
            // Act
            var baseline = Baseline.DeserializeFrom(TestRootDir, fs);
            // Assert
            var expected = new Baseline
            {
                Id = new BaselineId(TestBaselineHash),
                Timestamp = TestTimestamp,
                Data = TestSchemaData,
                ProviderConfigurationData = TestProviderConfigurationData,
                ServerInfo = TestServerVersionInfo
            };
            baseline.Should()
                .NotBeNull()
                .And.BeEquivalentTo(expected);
        }
    }
}
