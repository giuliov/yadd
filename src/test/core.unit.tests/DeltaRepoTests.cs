using FluentAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using yadd.core;

namespace core.unit.tests
{
    public class DeltaRepoTests : TestMockDataBase
    {
        [Fact]
        public void StageFile_CalledOnce_StageHasOneFile()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            var repo = new DeltaRepo(TestRootDir, fs);

            repo.StageFile(TestScriptFile1);

            fs.Directory.GetFiles($"{TestRootDir}\\staging").Should().HaveCount(2);
            fs.FileExists($"{TestRootDir}\\staging\\index").Should().BeTrue();
            fs.FileExists($"{TestRootDir}\\staging\\{TestScriptFile1Hash}").Should().BeTrue();
        }

        [Fact]
        public void StageFile_CalledTwiceWithSameFile_Fails()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            var repo = new DeltaRepo(TestRootDir, fs);

            repo.StageFile(TestScriptFile1);
            repo.Invoking(r => r.StageFile(TestScriptFile1))
                .Should().Throw<System.IO.IOException>();
        }

        [Fact]
        public void StageFile_CalledTwice_StageHasTwoFiles()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            fs.AddFile(TestScriptFile2, new MockFileData(TestScriptFile2Content));
            var repo = new DeltaRepo(TestRootDir, fs);

            repo.StageFile(TestScriptFile2);
            repo.StageFile(TestScriptFile1);

            fs.Directory.GetFiles($"{TestRootDir}\\staging").Should().HaveCount(3);
            fs.FileExists($"{TestRootDir}\\staging\\index").Should().BeTrue();
            fs.FileExists($"{TestRootDir}\\staging\\{TestScriptFile1Hash}").Should().BeTrue();
            fs.FileExists($"{TestRootDir}\\staging\\{TestScriptFile2Hash}").Should().BeTrue();
        }

        [Fact]
        public void UnstageFile_EmptyStage_Fails()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            var repo = new DeltaRepo(TestRootDir, fs);

            repo.Invoking(r => r.UnstageFile(TestScriptFile1))
                .Should().Throw<System.IO.IOException>();
        }

        [Fact]
        public void UnstageFile_MatchStaged_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            var repo = new DeltaRepo(TestRootDir, fs);
            repo.StageFile(TestScriptFile1);
            repo.UnstageFile(TestScriptFile1);

            fs.Directory.GetFiles($"{TestRootDir}\\staging").Should().ContainSingle();
            fs.FileExists($"{TestRootDir}\\staging\\index").Should().BeTrue();
        }

        [Fact]
        public void UnstageFile_DoesntMatchStaged_Fails()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            var repo = new DeltaRepo(TestRootDir, fs);
            repo.StageFile(TestScriptFile1);

            repo.Invoking(r => r.UnstageFile(TestScriptFile2))
                .Should().Throw<System.IO.IOException>();
        }

        [Fact]
        public void GetStagedFiles_StageIsEmpty_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            var repo = new DeltaRepo(TestRootDir, fs);

            repo.GetStagedFiles().Should().BeEmpty();
        }

        [Fact]
        public void GetStagedFiles_StageHasOneFile_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            var repo = new DeltaRepo(TestRootDir, fs);
            repo.StageFile(TestScriptFile1);

            var result = repo.GetStagedFiles();

            result.Should().HaveCount(1);
            result.Should().Contain(fs.Path.GetFileName(TestScriptFile1));
        }

        [Fact]
        public void GetStagedFiles_StageHasTwoFiles_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            fs.AddFile(TestScriptFile2, new MockFileData(TestScriptFile2Content));
            var repo = new DeltaRepo(TestRootDir, fs);
            repo.StageFile(TestScriptFile1);
            repo.StageFile(TestScriptFile2);

            var result = repo.GetStagedFiles();

            result.Should().HaveCount(2);
        }

        [Fact]
        public void ClearStagingArea_StageIsEmpty_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            var repo = new DeltaRepo(TestRootDir, fs);

            repo.ClearStagingArea();

            fs.Directory.GetFiles($"{TestRootDir}\\staging").Should().BeEmpty();
        }

        [Fact]
        public void ClearStagingArea_StageHasTwoFiles_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            fs.AddFile(TestScriptFile2, new MockFileData(TestScriptFile2Content));
            var repo = new DeltaRepo(TestRootDir, fs);
            repo.StageFile(TestScriptFile1);
            repo.StageFile(TestScriptFile2);

            repo.ClearStagingArea();

            fs.Directory.GetFiles($"{TestRootDir}\\staging").Should().BeEmpty();
        }

        [Fact]
        public void GetStagedScripts_StageHasTwoFiles_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            fs.AddFile(TestScriptFile2, new MockFileData(TestScriptFile2Content));
            var repo = new DeltaRepo(TestRootDir, fs);
            repo.StageFile(TestScriptFile1);
            repo.StageFile(TestScriptFile2);

            var result = repo.GetStagedScripts();

            result.Should()
                .HaveCount(2)
                .And.AllBeOfType<DeltaScript>();
        }

        [Fact]
        public void GetDelta_RepoHasOneDelta_Succeeds()
        {
            var fs = new MockFileSystem();
            var id = new DeltaId("a4f5d05f8f7432e3586a68c4f5ce7b01173e0bcd877441b0c6d6206735c4f2c1282e42753c0f355b173a5897d312c2383d87aa5956febab6d81611d298f79c6c");
            string deltaDir = $"{TestRootDir}\\delta\\{id.Filename}";
            string parentHash = new string('1', 128);
            fs.AddFile(fs.Path.Combine(deltaDir, "parent_baseline"), new MockFileData(parentHash));
            string commitMessage = "some_commit_message";
            fs.AddFile(fs.Path.Combine(deltaDir, "commit_message"), new MockFileData(commitMessage));
            var sid = new DeltaScriptId("61ba912ac17d2eb1f4fdd96c52b382e13079f22af81e4e4a70381536122d1233412fdc778c3e8f40b99c360d37c72d38924a4fad9bde844fc20b9a731726a87a");
            fs.AddFile(fs.Path.Combine(deltaDir, "index"), new MockFileData($"{sid.Filename} pippo {sid.Hash}"));
            fs.AddFile(fs.Path.Combine(deltaDir, sid.Filename), new MockFileData("SELECT @@VERSION"));
            var repo = new DeltaRepo(TestRootDir, fs);

            var delta = repo.GetDelta(id);

            delta.ParentBaselineId.Should().Be(new BaselineId(parentHash));
            delta.CommitMessage.Should().Be(commitMessage);
            delta.Scripts.Should().HaveCount(1);
            delta.Scripts[0].Name.Should().Be("pippo");
            delta.Scripts[0].Code.Should().Be("SELECT @@VERSION");
            delta.Id.Should().Be(id);
        }

        [Fact]
        public void GetDelta_RepoHasNoDeltas_Fails()
        {
            var fs = new MockFileSystem();
            var id = new DeltaId(TestHash1);
            var repo = new DeltaRepo(TestRootDir, fs);

            repo.Invoking(r => r.GetDelta(id))
                .Should().Throw<System.IO.IOException>();
        }

        [Fact]
        public void AddDelta_EmptyRepo_Succeeds()
        {
            string commitMessage = "some_commit_message";
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\staging");
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            var repo = new DeltaRepo(TestRootDir, fs);
            repo.StageFile(TestScriptFile1);

            var delta = repo.AddDelta(commitMessage, new BaselineId(TestHash1));

            var expectedId = new DeltaId("aa3cb08a6efcaab2de985c2835da0b5b05772a854ab5bb78f5080107e01e108762ad2a10ed352582cd4bbd07b413b5214a45316739f6ed210a8dd1180905c0cb");
            delta.Id.Should().Be(expectedId);
        }
    }
}
