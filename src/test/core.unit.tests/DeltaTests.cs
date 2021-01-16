using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Xunit;
using yadd.core;

namespace core.unit.tests
{
    public class DeltaTests : TestMockDataBase
    {
        [Fact]
        public void Serialize_ValidDelta_Succeeds()
        {
            // Arrange
            string commitMessage = "some_commit_message";
            var fs = new MockFileSystem();
            var delta = new Delta {
                Id = new DeltaId(TestHash2),
                CommitMessage = commitMessage,
                ParentBaselineId = new BaselineId(TestHash1),
                Scripts = new DeltaScript[]
                {
                    new DeltaScript { Name="pippo", Code=TestScriptFile1Content }
                }
            };
            // Act
            var id = delta.SerializeTo(TestRootDir, fs);
            // Assert
            var expected = new DeltaId("a9006a4d656de1bf85c9d8c3fbc3d5c0ce16ededc3f8eb1c1a6a07391343302c456627baf30c2f8ab709c71e4e59f8ac60d4a13295e320018151569b72b37e9f");
            id.Should().Be(expected);
            string deltaDir = $"{TestRootDir}\\{id.Filename}";
            fs.FileExists($"{deltaDir}\\parent_baseline").Should().BeTrue();
            fs.GetFile($"{deltaDir}\\parent_baseline").TextContents
                .Should().Be(TestHash1);
            fs.FileExists($"{deltaDir}\\commit_message").Should().BeTrue();
            fs.GetFile($"{deltaDir}\\commit_message").TextContents
                .Should().Be(commitMessage);
            fs.FileExists($"{deltaDir}\\index").Should().BeTrue();
            var index = fs.GetFile($"{deltaDir}\\index").TextContents.SplitLines();
            index.Should()
                .ContainSingle()
                .And.ContainMatch("* pippo *");
            var df = fs.Directory.EnumerateFiles(deltaDir)
                .Where(f => fs.Path.GetFileName(f).StartsWith(index[0].Substring(0, 10)))
                .FirstOrDefault();
            df.Should().NotBeNull();
            fs.GetFile(df).TextContents.Should().Be(TestScriptFile1Content);
        }

        [Fact]
        public void Deserialize_ValidDelta_Succeeds()
        {
            // Arrange
            var fs = new MockFileSystem();
            var id = new DeltaId("a4f5d05f8f7432e3586a68c4f5ce7b01173e0bcd877441b0c6d6206735c4f2c1282e42753c0f355b173a5897d312c2383d87aa5956febab6d81611d298f79c6c");
            string deltaDir = $"{TestRootDir}\\{id.Filename}";
            fs.AddFile(fs.Path.Combine(deltaDir, "parent_baseline"), new MockFileData(TestHash1));
            string commitMessage = "some_commit_message";
            fs.AddFile(fs.Path.Combine(deltaDir, "commit_message"), new MockFileData(commitMessage));
            var did = new DeltaScriptId(TestScriptFile1Hash);
            fs.AddFile(fs.Path.Combine(deltaDir, "index"), new MockFileData($"{did.Filename} pippo {did.Hash}"));
            fs.AddFile(fs.Path.Combine(deltaDir, did.Filename), new MockFileData("SELECT @@VERSION"));

            // Act
            var delta = Delta.DeserializeFrom(deltaDir, fs);
            // Assert
            delta.ParentBaselineId.Should().Be(new BaselineId(TestHash1));
            delta.CommitMessage.Should().Be(commitMessage);
            delta.Scripts.Should()
                .ContainSingle()
                .And.ContainEquivalentOf(new DeltaScript { Name = "pippo", Code = TestScriptFile1Content });
            delta.Id.Should().Be(id);
        }
    }
}
