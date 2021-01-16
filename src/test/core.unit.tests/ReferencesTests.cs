using FluentAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using yadd.core;

namespace core.unit.tests
{
    public class ReferencesTests : TestMockDataBase
    {
        [Fact]
        public void SetRootBaselineId_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            var id = new BaselineId(TestHash1);
            var references = new References(TestRootDir, fs);

            references.SetRootBaselineId(id);

            fs.FileExists($"{TestRootDir}\\reference\\root_baseline").Should().BeTrue();
            fs.GetFile($"{TestRootDir}\\reference\\root_baseline").TextContents.Should().Be(TestHash1);
            fs.FileExists($"{TestRootDir}\\current_baseline").Should().BeTrue();
            fs.GetFile($"{TestRootDir}\\current_baseline").TextContents.Should().Be(":main");
            fs.FileExists($"{TestRootDir}\\reference\\branches\\main").Should().BeTrue();
            fs.GetFile($"{TestRootDir}\\reference\\branches\\main").TextContents.Should().Be(TestHash1);
        }

        [Fact]
        public void GetRootBaselineId_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddFile($"{TestRootDir}\\reference\\root_baseline", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);

            var id = references.GetRootBaselineId();

            id.Should()
                .NotBeNull()
                .And.Be(new BaselineId(TestHash1));
        }

        [Fact]
        public void SetCurrent_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddFile($"{TestRootDir}\\current_baseline", new MockFileData(TestHash1));
            var id = new BaselineId(TestHash1);
            var references = new References(TestRootDir, fs);

            references.SetCurrent(id);

            fs.FileExists($"{TestRootDir}\\current_baseline").Should().BeTrue();
            fs.GetFile($"{TestRootDir}\\current_baseline").TextContents.Should().Be(TestHash1);
        }

        [Fact]
        public void GetCurrent_HasId_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddFile($"{TestRootDir}\\current_baseline", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);

            var id = references.GetCurrentBaselineId();

            id.Should().NotBeNull()
                .And.Be(new BaselineId(TestHash1));
        }

        [Fact]
        public void GetCurrent_HasBranch_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddFile($"{TestRootDir}\\current_baseline", new MockFileData(":mybranch"));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\mybranch", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);

            var id = references.GetCurrentBaselineId();

            id.Should()
                .NotBeNull()
                .And.Be(new BaselineId(TestHash1));
        }

        [Fact]
        public void GetCurrent_BrokenCurrent_ReturnsNull()
        {
            var fs = new MockFileSystem();
            fs.AddFile($"{TestRootDir}\\current_baseline", new MockFileData(":nonexistingbranch"));
            var references = new References(TestRootDir, fs);

            var id = references.GetCurrentBaselineId();

            id.Should().BeNull();
        }

        [Fact]
        public void GetAllBranches_NoBranches_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);

            var branches = references.GetAllBranches();

            branches.Should()
                .NotBeNull()
                .And.BeEmpty();
            fs.GetFile($"{TestRootDir}\\current_baseline").TextContents
                .Should().NotStartWith(":")
                .And.HaveLength(128); //HACK
        }

        [Fact]
        public void GetAllBranches_OneBranch_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", new MockFileData(":mybranch"));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\mybranch", new MockFileData("doesn't matter"));
            var references = new References(TestRootDir, fs);

            var branches = references.GetAllBranches();

            var expected = (new BaselineRef("mybranch"), true);
            branches.Should()
                .NotBeNull()
                .And.ContainSingle()
                .And.ContainEquivalentOf(expected);
        }

        [Fact]
        public void GetAllBranches_TwoBranches_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", new MockFileData(":mybranch"));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\mybranch", new MockFileData("doesn't matter"));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\anotherbranch", new MockFileData("doesn't matter"));
            var references = new References(TestRootDir, fs);

            var branches = references.GetAllBranches();

            var expected1 = (new BaselineRef("mybranch"), true);
            var expected2 = (new BaselineRef("anotherbranch"), false);
            branches.Should()
                .NotBeNull()
                .And.HaveCount(2)
                .And.ContainEquivalentOf(expected1)
                .And.ContainEquivalentOf(expected2);
        }

        [Fact]
        public void GetAllBranches_DetachedHead_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", new MockFileData(TestHash1));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\mybranch", new MockFileData("doesn't matter"));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\anotherbranch", new MockFileData("doesn't matter"));
            var references = new References(TestRootDir, fs);

            var branches = references.GetAllBranches();

            var expected1 = (new BaselineRef("mybranch"), false);
            var expected2 = (new BaselineRef("anotherbranch"), false);
            branches.Should()
                .NotBeNull()
                .And.HaveCount(2)
                .And.ContainEquivalentOf(expected1)
                .And.ContainEquivalentOf(expected2);
        }

        [Fact]
        public void SwitchToBranch_MainToExisting_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", ":main");
            fs.AddFile($"{TestRootDir}\\reference\\branches\\main", new MockFileData(TestHash1));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\mybranch", new MockFileData(TestHash2));
            var references = new References(TestRootDir, fs);
            var destBranch = new BaselineRef("mybranch");

            references.SwitchToBranch(destBranch);

            fs.GetFile($"{TestRootDir}\\current_baseline").TextContents.Should().Be(":mybranch");
        }

        [Fact]
        public void SwitchToBranch_MainToNonExisting_CreatesNew()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", ":main");
            fs.AddFile($"{TestRootDir}\\reference\\branches\\main", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);
            var destBranch = new BaselineRef("newbranch");

            references.SwitchToBranch(destBranch);

            fs.FileExists($"{TestRootDir}\\reference\\branches\\newbranch").Should().BeTrue();
            fs.GetFile($"{TestRootDir}\\reference\\branches\\newbranch").TextContents.Should().Be(TestHash1);
            fs.GetFile($"{TestRootDir}\\current_baseline").TextContents.Should().Be(":newbranch");
        }

        [Fact]
        public void DeleteBranch_NonCurrent_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", ":main");
            fs.AddFile($"{TestRootDir}\\reference\\branches\\main", new MockFileData(TestHash1));
            fs.AddFile($"{TestRootDir}\\reference\\branches\\anotherbranch", new MockFileData(TestHash2));
            var references = new References(TestRootDir, fs);
            var destBranch = new BaselineRef("anotherbranch");

            references.DeleteBranch(destBranch);

            fs.GetFile($"{TestRootDir}\\current_baseline").TextContents.Should().Be(":main");
            fs.FileExists($"{TestRootDir}\\reference\\branches\\anotherbranch").Should().BeFalse();
        }

        [Fact]
        public void DeleteBranch_Current_Fails()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", ":anotherbranch");
            fs.AddFile($"{TestRootDir}\\reference\\branches\\anotherbranch", new MockFileData(TestHash2));
            var references = new References(TestRootDir, fs);
            var destBranch = new BaselineRef("anotherbranch");

            references.Invoking(r => r.DeleteBranch(destBranch))
                .Should().Throw<System.Exception>();

            fs.GetFile($"{TestRootDir}\\current_baseline").TextContents.Should().Be(":anotherbranch");
            fs.FileExists($"{TestRootDir}\\reference\\branches\\anotherbranch").Should().BeTrue();
        }

        [Fact]
        public void DeleteBranch_NonExisting_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\branches");
            fs.AddFile($"{TestRootDir}\\current_baseline", ":main");
            fs.AddFile($"{TestRootDir}\\reference\\branches\\main", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);
            var destBranch = new BaselineRef("nonexistingbranch");

            references.DeleteBranch(destBranch);

            fs.GetFile($"{TestRootDir}\\current_baseline").TextContents.Should().Be(":main");
            fs.FileExists($"{TestRootDir}\\reference\\branches\\nonexistingbranch").Should().BeFalse();
        }

        [Fact]
        public void GetAllTags_NoTags_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            var references = new References(TestRootDir, fs);

            var tags = references.GetAllTags();

            tags.Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void GetAllTags_OneValidTag_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            fs.AddFile($"{TestRootDir}\\reference\\tags\\label1", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);

            var tags = references.GetAllTags();

            var expected = (new BaselineRef("label1"), new BaselineId(TestHash1));
            tags.Should()
                .NotBeNull()
                .And.ContainSingle()
                .And.ContainEquivalentOf(expected);
        }

        [Fact]
        public void GetAllTags_TwoValidTags_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            fs.AddFile($"{TestRootDir}\\reference\\tags\\label1", new MockFileData(TestHash1));
            fs.AddFile($"{TestRootDir}\\reference\\tags\\label2", new MockFileData(TestHash2));
            var references = new References(TestRootDir, fs);

            var tags = references.GetAllTags();

            var expected1 = (new BaselineRef("label1"), new BaselineId(TestHash1));
            var expected2 = (new BaselineRef("label2"), new BaselineId(TestHash2));
            tags.Should()
                .NotBeNull()
                .And.HaveCount(2)
                .And.ContainEquivalentOf(expected1)
                .And.ContainEquivalentOf(expected2);
        }

        [Fact]
        public void AddTag_NoTags_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            var references = new References(TestRootDir, fs);
            var tag = new BaselineRef("newtag");
            var target = new BaselineId(TestHash1);

            references.AddTag(tag, target);

            fs.FileExists($"{TestRootDir}\\reference\\tags\\newtag").Should().BeTrue();
            fs.GetFile($"{TestRootDir}\\reference\\tags\\newtag").TextContents.Should().Be(TestHash1);
        }

        [Fact]
        public void RemoveTag_LastExistingTag_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            fs.AddFile($"{TestRootDir}\\reference\\tags\\label1", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);
            var tag = new BaselineRef("label1");

            references.RemoveTag(tag);

            fs.FileExists($"{TestRootDir}\\reference\\tags\\label1")
                .Should().BeFalse();
            fs.Directory.GetFiles($"{TestRootDir}\\reference\\tags")
                .Should().BeEmpty();
        }

        [Fact]
        public void RemoveTag_ExistingTag_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            fs.AddFile($"{TestRootDir}\\reference\\tags\\label1", new MockFileData(TestHash1));
            fs.AddFile($"{TestRootDir}\\reference\\tags\\label2", new MockFileData(TestHash2));
            var references = new References(TestRootDir, fs);
            var tag = new BaselineRef("label2");

            references.RemoveTag(tag);

            fs.FileExists($"{TestRootDir}\\reference\\tags\\label1")
                .Should().BeTrue();
            fs.FileExists($"{TestRootDir}\\reference\\tags\\label2")
                .Should().BeFalse();
            fs.Directory.GetFiles($"{TestRootDir}\\reference\\tags")
                .Should().ContainSingle();
        }

        [Fact]
        public void RemoveTag_NonExistingTag_Fails()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            fs.AddFile($"{TestRootDir}\\reference\\tags\\label1", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);
            var tag = new BaselineRef("nonlabel");

            references.Invoking(r => r.RemoveTag(tag))
                .Should().Throw<System.Exception>();

            fs.FileExists($"{TestRootDir}\\reference\\tags\\label1")
                .Should().BeTrue();
            fs.Directory.GetFiles($"{TestRootDir}\\reference\\tags")
                .Should().ContainSingle();
        }

        [Fact]
        public void RemoveTag_EmptySet_Fails()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            var references = new References(TestRootDir, fs);
            var tag = new BaselineRef("nonlabel");

            references.Invoking(r => r.RemoveTag(tag))
                .Should().Throw<System.Exception>();

            fs.Directory.GetFiles($"{TestRootDir}\\reference\\tags")
                .Should().BeEmpty();
        }

        [Fact]
        public void Resolve_ExistingTag_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\baseline");
            fs.AddDirectory($"{TestRootDir}\\reference");
            fs.AddDirectory($"{TestRootDir}\\reference\\tags");
            fs.AddFile($"{TestRootDir}\\reference\\tags\\label1", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);
            var bref = new BaselineRef("label1");

            var id = references.Resolve(bref);

            id.Should().NotBeNull();
            id.Hash.Should().Be(TestHash1);
        }

        [Fact]
        public void Resolve_ExistingBaseline_PartialMatch_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\baseline");
            fs.AddDirectory($"{TestRootDir}\\baseline\\{TestHash1}");
            fs.AddFile($"{TestRootDir}\\baseline\\{TestHash1}\\id", new MockFileData(TestHash1));
            var references = new References(TestRootDir, fs);
            var bref = new BaselineRef("111111");

            var id = references.Resolve(bref);

            id.Should().NotBeNull();
            id.Hash.Should().Be(TestHash1);
        }

        [Fact]
        public void Resolve_NonExistingBaseline_PartialMatch_Fails()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\baseline");
            fs.AddDirectory($"{TestRootDir}\\baseline\\{TestHash1}");
            var references = new References(TestRootDir, fs);
            var bref = new BaselineRef("222");

            var id = references.Resolve(bref);

            id.Should().BeNull();
        }

        [Fact]
        public void Resolve_TooManyMatches_Fails()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory($"{TestRootDir}\\baseline");
            fs.AddDirectory($"{TestRootDir}\\baseline\\aaa1");
            fs.AddDirectory($"{TestRootDir}\\baseline\\aaa2");
            var references = new References(TestRootDir, fs);
            var bref = new BaselineRef("aaa");

            references.Invoking(r => r.Resolve(bref))
                .Should().Throw<System.Exception>();
        }
    }
}
