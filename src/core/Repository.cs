using OneOf;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace yadd.core
{
    using HistoryItem = OneOf<Baseline, Delta>;

    public class Repository
    {
        const string RootName = ".yadd";

        private IFileSystem FS { get; init; }
        private References References { get; init; }
        private BaselineRepo BaselineRepo { get; init; }
        private DeltaRepo DeltaRepo { get; init; }

        public static Repository Init(Baseline baseline, IProvider provider) { return Init(baseline, provider, new FileSystem()); }
        public static Repository Init(Baseline baseline, IProvider provider, IFileSystem fileSystem)
        {
            if (fileSystem.Directory.Exists(RootName))
                throw new Exception("Repository already initialized");

            var repoDir = fileSystem.Directory.CreateDirectory(RootName);
            var info = new YaddRepoInfo(repoDir.FullName, fileSystem);
            info.Write(provider.GetServerVersion());
            var repo = new Repository
            {
                FS = fileSystem,
                // create initial structure
                References = new References(repoDir, fileSystem),
                BaselineRepo = new BaselineRepo(repoDir, fileSystem),
                DeltaRepo = new DeltaRepo(repoDir, fileSystem),
            };
            // HACK
            repo.BaselineRepo.AddRootBaseline(baseline, repo.References);
            return repo;
        }

        public static Repository FindUpward() { return FindUpward(new FileSystem()); }
        public static Repository FindUpward(IFileSystem fileSystem)
        {
            string current = fileSystem.Directory.GetCurrentDirectory();
            while (current != null && !fileSystem.Directory.Exists(fileSystem.Path.Combine(current, RootName)))
            {
                current = fileSystem.Directory.GetParent(current)?.FullName;
            }
            if (current == null) ThrowInvalidRepo();
            string rootDir = fileSystem.Path.Combine(current, RootName);

            var info = new YaddRepoInfo(rootDir, fileSystem);
            if (!info.Read()) ThrowInvalidRepo();

            return new Repository
            {
                FS = fileSystem,
                References = new References(rootDir, fileSystem),
                BaselineRepo = new BaselineRepo(rootDir, fileSystem),
                DeltaRepo = new DeltaRepo(rootDir, fileSystem),
            };

            static void ThrowInvalidRepo()
            {
                throw new Exception("Cannot find a valid repo");
            }
        }

        public Baseline GetMatchingBaseline(BaselineRef bref)
        {
            var id = References.Resolve(bref);
            return BaselineRepo.GetBaseline(id);
        }

        public Baseline GetRootBaseline()
        {
            return BaselineRepo.GetBaseline(References.GetRootBaselineId());
        }

        public Baseline GetCurrentBaseline()
        {
            return BaselineRepo.GetBaseline(References.GetCurrentBaselineId());
        }

        public (bool found, BaselineId id) FindMatch(Baseline baseline)
        {
            return BaselineRepo.FindMatch(baseline);
        }

        public void Stage(string filename)
        {
            DeltaRepo.StageFile(filename);
        }

        public void Unstage(string filename)
        {
            DeltaRepo.UnstageFile(filename);
        }

        public IEnumerable<string> GetStaged()
        {
            return DeltaRepo.GetStagedFiles();
        }

        public IEnumerable<DeltaScript> GetStagedScripts()
        {
            return DeltaRepo.GetStagedScripts();
        }

        public (BaselineId parent, BaselineId @new) Commit(string message, Baseline newBaseline)
        {
            var parent = GetCurrentBaseline();

            // sanity check
            if (parent.Data == newBaseline.Data) throw new Exception("No schema changes: aborting commit");

            var delta = DeltaRepo.AddDelta(message, parent.Id);

            DeltaRepo.ClearStagingArea();

            var newBaselineId = BaselineRepo.AddBaseline(newBaseline, delta.Id, parent.Id);

            References.SetCurrent(newBaselineId);

            return (parent: parent.Id, @new: newBaselineId);
        }

        public IEnumerable<HistoryItem> GetLogBetween(Baseline initialBaseline, Baseline finalBaseline)
        {
            var log = new Stack<HistoryItem>();

            // move backward through history
            var currentId = finalBaseline.Id;
            while (!currentId.Equals(initialBaseline.Id))
            {
                var currentBaseline = BaselineRepo.GetBaseline(currentId);
                log.Push(currentBaseline);

                var deltaId = BaselineRepo.GetDelta(currentId);
                if (deltaId==null)
                    break;

                var delta = DeltaRepo.GetDelta(deltaId);

                log.Push(delta);

                currentId = delta.ParentBaselineId;
            }

            log.Push(initialBaseline);

            return log;
        }

        public IEnumerable<HistoryItem> GetFullHistory()
        {
            // move backward through history
            var currentId = References.GetCurrentBaselineId();
            while (currentId != null)
            {
                var currentBaseline = BaselineRepo.GetBaseline(currentId);
                yield return currentBaseline;

                var deltaId = BaselineRepo.GetDelta(currentId);
                if (deltaId == null)
                    break;

                var delta = DeltaRepo.GetDelta(deltaId);

                yield return delta;

                currentId = delta.ParentBaselineId;
            }
        }

        public void SwitchTo(BaselineRef newBranch)
        {
            References.SwitchToBranch(newBranch);
        }

        public void RemoveBranch(BaselineRef branch)
        {
            References.DeleteBranch(branch);
        }

        public IEnumerable<(BaselineRef name, bool current)> GetAllBranches()
        {
            return References.GetAllBranches();
        }

        public IEnumerable<(BaselineRef tag, BaselineId id)> GetAllTags()
        {
            return References.GetAllTags();
        }

        public void AddTag(BaselineRef tag, BaselineRef target)
        {
            References.AddTag(tag, References.Resolve(target));
        }

        public void RemoveTag(BaselineRef tag)
        {
            References.RemoveTag(tag);
        }
    }
}
