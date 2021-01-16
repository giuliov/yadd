using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Xunit;
using yadd.core;

namespace core.unit.tests
{
    public class BaselineRepoTests : TestMockDataBase
    {
        [Fact]
        public void GetBaseline_EmptyRepo_Fails()
        {
            var fs = new MockFileSystem();
            var id = new BaselineId(TestHash1);
            var repo = new BaselineRepo(TestRootDir, fs);

            repo.Invoking(r => r.GetBaseline(id))
                .Should().Throw<System.IO.IOException>();
        }

        [Fact]
        public void GetBaseline_Exists_Succeeds()
        {
            var fs = GetPopulatedRepo();
            var id = new BaselineId(TestBaselineHash);
            var repo = new BaselineRepo(TestRootDir, fs);

            var baseline = repo.GetBaseline(id);

            baseline.Should().NotBeNull();
        }

        [Fact]
        public void GetBaseline_NonExistingId_Fails()
        {
            var fs = GetPopulatedRepo();
            var id = new BaselineId(TestHash1);
            var repo = new BaselineRepo(TestRootDir, fs);

            repo.Invoking(r => r.GetBaseline(id))
                .Should().Throw<System.IO.IOException>();
        }

        [Fact]
        public void FindMatch_InvalidPattern_NotFound()
        {
            var fs = new MockFileSystem();
            var repo = new BaselineRepo(TestRootDir, fs);
            var baseline = new Baseline();

            repo.Invoking(r => r.FindMatch(baseline))
                .Should().Throw<System.ArgumentNullException>();
        }

        [Fact]
        public void FindMatch_EmptyPattern_EmptyRepo_NotFound()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\baseline");
            var repo = new BaselineRepo(TestRootDir, fs);
            var baseline = new Baseline
            {
                Data = string.Empty
            };

            var result = repo.FindMatch(baseline);

            result.Should().NotBeNull();
            result.found.Should().BeFalse();
        }

        [Fact]
        public void FindMatch_EmptyPattern_PopulatedRepo_NotFound()
        {
            var fs = GetPopulatedRepo();
            var repo = new BaselineRepo(TestRootDir, fs);
            var baseline = new Baseline
            {
                Data = string.Empty
            };

            var result = repo.FindMatch(baseline);

            result.Should().NotBeNull();
            result.found.Should().BeFalse();
        }

        [Fact]
        public void FindMatch_MatchingPattern_PopulatedRepo_Succeeds()
        {
            var fs = GetPopulatedRepo();
            var repo = new BaselineRepo(TestRootDir, fs);
            var baseline = new Baseline
            {
                Data = TestSchemaData,
                ProviderConfigurationData = TestProviderConfigurationData,
                ServerInfo = TestServerVersionInfo,
            };

            var result = repo.FindMatch(baseline);

            result.Should().NotBeNull();
            var expected = (true, new BaselineId(TestBaselineHash));
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void FindMatch_ValidPattern_EmptyRepo_NotFound()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\baseline");
            var repo = new BaselineRepo(TestRootDir, fs);
            var baseline = new Baseline
            {
                Data = TestSchemaData,
                ProviderConfigurationData = TestProviderConfigurationData,
                ServerInfo = TestServerVersionInfo,
            };

            var result = repo.FindMatch(baseline);

            result.Should().NotBeNull();
            result.found.Should().BeFalse();
        }

        [Fact]
        public void GetDelta_RepoHasOneDelta_Succeeds()
        {
            var fs = GetPopulatedRepo();
            var did = new DeltaId("a4f5d05f8f7432e3586a68c4f5ce7b01173e0bcd877441b0c6d6206735c4f2c1282e42753c0f355b173a5897d312c2383d87aa5956febab6d81611d298f79c6c");
            fs.AddFile($"{TestRootDir}\\baseline\\{TestBaselineHash.Substring(0, 38)}\\delta", new MockFileData(did.Hash));
            var repo = new BaselineRepo(TestRootDir, fs);
            var id = new BaselineId(TestBaselineHash);

            var result = repo.GetDelta(id);

            result.Should().Be(did);
        }

        [Fact]
        public void GetDelta_RepoHasNoDeltas_ReturnsNull()
        {
            var fs = GetPopulatedRepo();
            var repo = new BaselineRepo(TestRootDir, fs);
            var id = new BaselineId(TestBaselineHash);

            var result = repo.GetDelta(id);

            result.Should().BeNull();
        }

        [Fact]
        public void AddRootBaseline_RepoIsEmpty_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            var repo = new BaselineRepo(TestRootDir, fs);
            var baseline = new Baseline
            {
                Timestamp = TestTimestamp,
                Data = TestSchemaData,
                ProviderConfigurationData = TestProviderConfigurationData,
                ServerInfo = TestServerVersionInfo,
            };
            var references = new References(TestRootDir, fs);

            repo.AddRootBaseline(baseline, references);

            fs.FileExists($"{TestRootDir}\\baseline\\{TestBaselineHash.Substring(0, 38)}\\id")
                .Should().BeTrue();
        }

        [Fact]
        public void AddRootBaseline_RepoIsNotEmpty_Fails()
        {
            var fs = GetPopulatedRepo();
            fs.AddFile($"{TestRootDir}\\reference\\root_baseline", new MockFileData(TestSchemaHash));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\main", new MockFileData(TestSchemaHash));
            var repo = new BaselineRepo(TestRootDir, fs);
            var baseline = new Baseline();
            var references = new References(TestRootDir, fs);

            repo.Invoking(r => r.AddRootBaseline(baseline, references))
                .Should().Throw<Exception>();
        }

        [Fact]
        public void AddBaseline_RepoHasRoot_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddDirectory($"{TestRootDir}\\staging");
            var repo = new BaselineRepo(TestRootDir, fs);
            var rootBaseline = new Baseline
            {
                Data = string.Empty,
                ProviderConfigurationData = string.Empty,
                ServerInfo = TestServerVersionInfo,
            };
            var references = new References(TestRootDir, fs);
            repo.AddRootBaseline(rootBaseline, references);
            var rootId = references.GetRootBaselineId();
            string commitMessage = "some_commit_message";
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            var deltaRepo = new DeltaRepo(TestRootDir, fs);
            deltaRepo.StageFile(TestScriptFile1);
            var delta = deltaRepo.AddDelta(commitMessage, new BaselineId(TestHash1));
            var baseline = new Baseline
            {
                Timestamp = TestTimestamp,
                Data = TestSchemaData,
                ProviderConfigurationData = TestProviderConfigurationData,
                ServerInfo = TestServerVersionInfo,
            };

            repo.AddBaseline(baseline, delta.Id, rootId);

            string addedPath = $"{TestRootDir}\\baseline\\{TestBaselineHash.Substring(0, 38)}";
            fs.FileExists($"{addedPath}\\id")
                .Should().BeTrue();
            fs.FileExists($"{addedPath}\\parent_baseline")
                .Should().BeTrue();
            fs.GetFile($"{addedPath}\\parent_baseline").TextContents
                .Should().Be(rootId.Hash);
            fs.FileExists($"{addedPath}\\delta")
                .Should().BeTrue();
            fs.GetFile($"{addedPath}\\delta").TextContents
                .Should().Be("aa3cb08a6efcaab2de985c2835da0b5b05772a854ab5bb78f5080107e01e108762ad2a10ed352582cd4bbd07b413b5214a45316739f6ed210a8dd1180905c0cb");
        }
    }
}
