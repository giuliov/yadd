using FluentAssertions;
using OneOf;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;
using yadd.core;

namespace core.unit.tests
{
    public class RepositoryTests : TestMockDataBase
    {
        [Fact]
        public void Init_EmptyDirectory_Succeeds()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");
            var baseline = new Baseline
            {
                ProviderConfigurationData = provider.ProviderConfigurationData,
                Timestamp = TestTimestamp,
                ServerInfo = provider.GetServerVersion(),
                Data = string.Empty,
            };

            fs.Invoking(x => 
                Repository.Init(baseline, provider, fs)
            ).Should().NotThrow();
        }

        [Fact]
        public void Init_AlreadyInitialized_Fails()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            fs.AddDirectory($"{TestRootDir}\\.yadd");
            var provider = new MockDbProvider(fs, "providers.toml");
            var baseline = new Baseline();

            fs.Invoking(x =>
                Repository.Init(baseline, provider, fs))
            .Should().Throw<Exception>();
        }

        [Fact]
        public void FindUpward_SameDirectory_NotInitialized_Fails()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);

            fs.Invoking(x =>
                Repository.FindUpward(fs))
            .Should().Throw<System.Exception>();
        }

        [Fact]
        public void FindUpward_DirectoryDownBelow_NotInitialized_Fails()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, $"{TestRootDir}\\a\\b\\c");

            fs.Invoking(x =>
                Repository.FindUpward(fs))
            .Should().Throw<System.Exception>();
        }

        MockFileSystem GetAlreadyInitialized(string baseDir)
        {
            var fs = GetFullyPopulatedRepo(baseDir);
            //fs.AddFile($"{baseDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml));
            return fs;
        }

        [Fact]
        public void FindUpward_SameDirectory_AlreadyInitialized_Succeeds()
        {
            var fs = GetAlreadyInitialized($"{TestRootDir}\\.yadd");
            fs.Directory.SetCurrentDirectory(TestRootDir);

            var repo = Repository.FindUpward(fs);

            repo.Should().NotBeNull();
        }

        [Fact]
        public void FindUpward_DirectoryDownBelow_AlreadyInitialized_Succeeds()
        {
            var fs = GetAlreadyInitialized($"{TestRootDir}\\.yadd");
            fs.Directory.SetCurrentDirectory($"{TestRootDir}\\a\\b\\c");

            var repo = Repository.FindUpward(fs);

            repo.Should().NotBeNull();
        }

        (Repository repo, Baseline rootBaseline, MockFileSystem fs) MakeRepoWithInitialBaseline()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");
            var rootBaseline = new Baseline
            {
                ProviderConfigurationData = provider.ProviderConfigurationData,
                Timestamp = TestTimestamp,
                ServerInfo = provider.GetServerVersion(),
                Data = TestSchemaData,
            };
            var repo = Repository.Init(rootBaseline, provider, fs);
            //HACK
            rootBaseline.Id = new BaselineId(TestBaselineHash);
            return (repo, rootBaseline, fs);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetRootBaseline_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, _) = MakeRepoWithInitialBaseline();

            var result = repo.GetRootBaseline();

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(rootBaseline);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetCurrentBaseline_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, _) = MakeRepoWithInitialBaseline();

            var result = repo.GetCurrentBaseline();

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(rootBaseline);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetMatchingBaseline_PartialId_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, _) = MakeRepoWithInitialBaseline();
            var bref = new BaselineRef(TestBaselineSerializeName.Substring(0, 4));

            var result = repo.GetMatchingBaseline(bref);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(rootBaseline);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FindMatch_Matches_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, _) = MakeRepoWithInitialBaseline();

            var result = repo.FindMatch(rootBaseline);

            result.Should().NotBeNull();
            result.found.Should().BeTrue();
            result.id.Filename.Should().Be(TestBaselineSerializeName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FindMatch_NoMatch()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var query = new Baseline
            {
                Data = string.Empty,
            };

            var result = repo.FindMatch(query);

            result.Should().NotBeNull();
            result.found.Should().BeFalse();
            result.id.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StageƐGetStaged_Succeeds()
        {
            (Repository repo, Baseline _, MockFileSystem fs) = MakeRepoWithInitialBaseline();
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));

            repo.Stage(TestScriptFile1);

            var result = repo.GetStaged();

            result.Should().ContainSingle();
            result.Should().Contain(fs.Path.GetFileName(TestScriptFile1));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StageƐUnstageƐGetStaged_Succeeds()
        {
            (Repository repo, Baseline _, MockFileSystem fs) = MakeRepoWithInitialBaseline();
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            fs.AddFile(TestScriptFile2, new MockFileData(TestScriptFile2Content));

            repo.Stage(TestScriptFile1);
            repo.Stage(TestScriptFile2);
            repo.Unstage(TestScriptFile1);

            var result = repo.GetStaged();

            result.Should().ContainSingle();
            result.Should().Contain(fs.Path.GetFileName(TestScriptFile2));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Unstage_NoStaged_Fails()
        {
            (Repository repo, Baseline _, MockFileSystem fs) = MakeRepoWithInitialBaseline();
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));

            repo.Invoking(r => r.Unstage(TestScriptFile1))
                .Should().Throw<Exception>();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StageƐCommit_NoChangesOnDb_Fails()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");
            var rootBaseline = new Baseline
            {
                ProviderConfigurationData = provider.ProviderConfigurationData,
                Timestamp = TestTimestamp,
                ServerInfo = provider.GetServerVersion(),
                Data = TestSchemaData,
            };
            var repo = Repository.Init(rootBaseline, provider, fs);
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));

            repo.Stage(TestScriptFile1);
            var nowBaseline = new Baseline
            {
                ProviderConfigurationData = provider.ProviderConfigurationData,
                //Timestamp = TestTimestamp,
                ServerInfo = provider.GetServerVersion(),
                Data = TestSchemaData,
            };

            repo.Invoking(
                r => r.Commit("some_commit_message", nowBaseline))
            .Should().Throw<Exception>();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StageƐCommit_Succeeds()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");
            var rootBaseline = new Baseline
            {
                ProviderConfigurationData = provider.ProviderConfigurationData,
                Timestamp = TestTimestamp,
                ServerInfo = provider.GetServerVersion(),
                Data = TestSchemaData,
            };
            var repo = Repository.Init(rootBaseline, provider, fs);
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));

            repo.Stage(TestScriptFile1);
            var nowBaseline = new Baseline
            {
                ProviderConfigurationData = provider.ProviderConfigurationData,
                Timestamp = TestTimestamp,
                ServerInfo = provider.GetServerVersion(),
                Data = "some different schema",
            };

            var result = repo.Commit("some_commit_message", nowBaseline);

            result.Should().NotBeNull();
            result.parent.Should().Be(new BaselineId(TestBaselineHash));
            result.@new.Should().Be(new BaselineId("e46435602fa0c9d121d28a5450b59d33fcb5c38074aadd5151d5b66ee94b21349231f37d6840d01200552ec387353f6627470165ae9faba94b6327ce1fe5660f"));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetFullHistory_InitializedRepo_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, MockFileSystem _) = MakeRepoWithInitialBaseline();

            var result = repo.GetFullHistory();

            rootBaseline.Id = new BaselineId(TestBaselineHash);
            result.Should().ContainSingle();
            result.FirstOrDefault().IsT0.Should().BeTrue();
            result.FirstOrDefault().AsT0.Should().BeEquivalentTo(rootBaseline);
        }

        (Repository repo, Baseline rootBaseline, Baseline currentBaseline, MockFileSystem fs) MakeRepoWithInitialAndAnotherBaseline()
        {
            var fsContent = new Dictionary<string, MockFileData> {
                { $"{TestRootDir}\\providers.toml", new MockFileData(MockDbProvider.ProvidersToml) }
            };
            var fs = new MockFileSystem(fsContent, TestRootDir);
            var provider = new MockDbProvider(fs, "providers.toml");
            var rootBaseline = new Baseline
            {
                ProviderConfigurationData = provider.ProviderConfigurationData,
                Timestamp = TestTimestamp,
                ServerInfo = provider.GetServerVersion(),
                Data = TestSchemaData,
            };
            var repo = Repository.Init(rootBaseline, provider, fs);
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));
            repo.Stage(TestScriptFile1);
            var currentBaseline = new Baseline
            {
                ProviderConfigurationData = provider.ProviderConfigurationData,
                Timestamp = TestTimestamp,
                ServerInfo = provider.GetServerVersion(),
                Data = "some different schema",
            };
            repo.Commit("some_commit_message", currentBaseline);

            //HACK
            rootBaseline.Id = new BaselineId(TestBaselineHash);
            currentBaseline.Id = new BaselineId("e46435602fa0c9d121d28a5450b59d33fcb5c38074aadd5151d5b66ee94b21349231f37d6840d01200552ec387353f6627470165ae9faba94b6327ce1fe5660f");

            return (repo, rootBaseline, currentBaseline, fs);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetFullHistory_AfterOneCommit_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, Baseline currentBaseline, MockFileSystem _) = MakeRepoWithInitialAndAnotherBaseline();
            var result = repo.GetFullHistory();

            result.Should().NotBeEmpty();
            result.Should().HaveCount(3);
            // OneOf is too hard for FluentAssertion
            var a = result.ToArray();
            a[0].IsT0.Should().BeTrue();
            a[1].IsT1.Should().BeTrue();
            a[2].IsT0.Should().BeTrue();
            a[0].AsT0.Should().BeEquivalentTo(currentBaseline);
            // TODO a[1].AsT1.Should().BeEquivalentTo(delta);
            a[2].AsT0.Should().BeEquivalentTo(rootBaseline);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetLogBetween_SameCommit_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, MockFileSystem _) = MakeRepoWithInitialBaseline();

            var result = repo.GetLogBetween(rootBaseline, rootBaseline);

            result.Should().ContainSingle();
            var a = result.ToArray();
            a[0].IsT0.Should().BeTrue();
            a[0].AsT0.Should().BeEquivalentTo(rootBaseline);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetLogBetween_InitialAndCurrentCommit_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, Baseline currentBaseline, MockFileSystem _) = MakeRepoWithInitialAndAnotherBaseline();

            var result = repo.GetLogBetween(rootBaseline, currentBaseline);

            result.Should().NotBeEmpty();
            result.Should().HaveCount(3);
            // OneOf is too hard for FluentAssertion
            var a = result.ToArray();
            a[0].IsT0.Should().BeTrue();
            a[1].IsT1.Should().BeTrue();
            a[2].IsT0.Should().BeTrue();
            a[0].AsT0.Should().BeEquivalentTo(rootBaseline);
            // TODO a[1].AsT1.Should().BeEquivalentTo(delta);
            a[2].AsT0.Should().BeEquivalentTo(currentBaseline);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetAllBranches_InitializedRepo_Succeeds()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var brefMain = new BaselineRef("main");

            var result = repo.GetAllBranches();

            (BaselineRef, bool) expected = (brefMain, true);
            result.Should().ContainSingle();
            result.Should().ContainEquivalentOf(expected);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SwitchTo_FromMainToNewBranch_Succeeds()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var brefMain = new BaselineRef("main");
            var bref1 = new BaselineRef("anotherbranch");

            repo.SwitchTo(bref1);
            var result = repo.GetAllBranches();

            (BaselineRef, bool) expected0 = (brefMain, false);
            (BaselineRef, bool) expected1 = (bref1, true);
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(expected0);
            result.Should().ContainEquivalentOf(expected1);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SwitchTo_FromMainToNewBranchAndBack_Succeeds()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var brefMain = new BaselineRef("main");
            var bref1 = new BaselineRef("anotherbranch");

            repo.SwitchTo(bref1);
            repo.SwitchTo(brefMain);
            var result = repo.GetAllBranches();

            (BaselineRef, bool) expected0 = (brefMain, true);
            (BaselineRef, bool) expected1 = (bref1, false);
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(expected0);
            result.Should().ContainEquivalentOf(expected1);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SwitchToƐRemoveBranch_FromMainToNewBranchAndBack_Succeeds()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var brefMain = new BaselineRef("main");
            var bref1 = new BaselineRef("anotherbranch");

            repo.SwitchTo(bref1);
            repo.SwitchTo(brefMain);
            repo.RemoveBranch(bref1);
            var result = repo.GetAllBranches();

            (BaselineRef, bool) expected = (brefMain, true);
            result.Should().ContainSingle();
            result.Should().ContainEquivalentOf(expected);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemoveBranch_Main_Fails()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var brefMain = new BaselineRef("main");

            repo.Invoking(r => r.RemoveBranch(brefMain))
                .Should().Throw<Exception>();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemoveTag_NonExisting_Fails()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var brefTag = new BaselineRef("nonexistingtag");

            repo.Invoking(r => r.RemoveTag(brefTag))
                .Should().Throw<Exception>();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemoveTag_NonExisting2_Fails()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var brefTag = new BaselineRef("main");

            repo.Invoking(r => r.RemoveTag(brefTag))
                .Should().Throw<Exception>();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetAllTags_NoTags_Succeeds()
        {
            (Repository repo, _, _) = MakeRepoWithInitialBaseline();
            var brefTag = new BaselineRef("main");

            var result = repo.GetAllTags();

            result.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void AddTagƐGetAllTags_OneTag_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, _) = MakeRepoWithInitialBaseline();
            var brefTag = new BaselineRef("main");

            repo.AddTag(brefTag, new BaselineRef(rootBaseline.Id.Displayname));
            var result = repo.GetAllTags();

            (BaselineRef, BaselineId) expected = (brefTag, rootBaseline.Id);
            result.Should().ContainSingle();
            result.Should().ContainEquivalentOf(expected);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void AddTagƐRemoveTag_LeavesNoTags_Succeeds()
        {
            (Repository repo, Baseline rootBaseline, _) = MakeRepoWithInitialBaseline();
            var brefTag = new BaselineRef("main");

            repo.AddTag(brefTag, new BaselineRef(rootBaseline.Id.Displayname));
            repo.RemoveTag(brefTag);
            var result = repo.GetAllTags();

            result.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetStagedScripts_NothingStaged_Succeeds()
        {
            (Repository repo, Baseline _, MockFileSystem fs) = MakeRepoWithInitialBaseline();

            var result = repo.GetStagedScripts();

            result.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StageƐGetStagedScripts_Succeeds()
        {
            (Repository repo, Baseline _, MockFileSystem fs) = MakeRepoWithInitialBaseline();
            fs.AddFile(TestScriptFile1, new MockFileData(TestScriptFile1Content));

            repo.Stage(TestScriptFile1);

            var result = repo.GetStagedScripts();

            var expected = new DeltaScript { Name = fs.Path.GetFileName(TestScriptFile1), Code = TestScriptFile1Content };
            result.Should().ContainSingle();
            result.Should().ContainEquivalentOf(expected);
        }
    }
}

