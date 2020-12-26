using OneOf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace yadd.core
{
    using HistoryItem = OneOf<Baseline, Delta>;

    public class Repository
    {
        const string RootName = ".yadd";
        const string InfoName = "info";
        static readonly Version RepoFormat = new Version(0, 1);

        public string Root { get; init; }
        private string BaselineDir { get; init; }
        private string DeltaDir { get; init; }
        private string StagingDir { get; init; }

        protected string CurrentBaselinePath => Path.Combine(Root, "current_baseline");
        protected string StagingIndexPath => Path.Combine(StagingDir, "index");

        public static Repository Init(Baseline baseline)
        {
            if (Directory.Exists(RootName))
                throw new Exception("Repository already initialized");

            var repoDir = Directory.CreateDirectory(RootName);
            File.WriteAllText(Path.Combine(repoDir.FullName, InfoName), $"{RepoFormat} yadd");
            var repo = new Repository {
                Root = repoDir.FullName,
                // create initial structure
                BaselineDir = repoDir.CreateSubdirectory("baseline").FullName,
                DeltaDir = repoDir.CreateSubdirectory("delta").FullName,
                StagingDir = repoDir.CreateSubdirectory("staging").FullName
            };
            var rootId = repo.AddBaseline(baseline);
            rootId.Write(Path.Combine(repoDir.FullName, "root_baseline"));
            return repo;
        }

        public static Repository FindUpward()
        {
            string current = Directory.GetCurrentDirectory();
            while (current != null && !Directory.Exists(Path.Combine(current, RootName)))
            {
                current = Directory.GetParent(current).FullName;
            }
            if (current == null) ThrowInvalidRepo();
            string[] info = File.ReadAllLines(Path.Combine(current, RootName, InfoName));
            if (info.Length < 1) ThrowInvalidRepo();
            string[] headLine = info[0].Split(' ');
            if (!Version.TryParse(headLine[0], out Version v)) ThrowInvalidRepo();
            if (v!=RepoFormat) ThrowInvalidRepo();
            string rootDir = Path.Combine(current, RootName);
            return new Repository
            {
                Root = rootDir,
                BaselineDir = Path.Combine(rootDir,"baseline"),
                DeltaDir = Path.Combine(rootDir, "delta"),
                StagingDir = Path.Combine(rootDir, "staging")
            };

            static void ThrowInvalidRepo()
            {
                throw new Exception("Cannot find a valid repo");
            }
        }

        private BaselineId GetCurrentBaselineId()
        {
            return ObjectId.Read<BaselineId>(CurrentBaselinePath);
        }

        private BaselineId GetRootBaselineId()
        {
            return ObjectId.Read<BaselineId>(Path.Combine(Root, "root_baseline"));
        }

        private BaselineId AddBaseline(Baseline baseline, DeltaId deltaId = null)
        {
            baseline.ParentId = GetCurrentBaselineId();

            string jsonString = JsonSerializer.Serialize(baseline);
            string hash = Hasher.GetHash(jsonString);
            var id = new BaselineId(hash);

            string baselineDir = Directory.CreateDirectory(Path.Combine(BaselineDir, id.Filename)).FullName;
            File.WriteAllText(Path.Combine(baselineDir, "schema.json"), jsonString);
            var parentId = GetCurrentBaselineId();
            if (parentId != null) parentId.Write(Path.Combine(baselineDir, "parent_baseline"));
            if (deltaId != null) deltaId.Write(Path.Combine(baselineDir, "delta"));
            id.Write(CurrentBaselinePath);

            return id;
        }

        public Baseline GetMatchingBaseline(string idMatch)
        {
            var matches = Directory.GetDirectories(BaselineDir, idMatch + "*");
            if (matches.Length != 1) throw new Exception($"Cannot find Baseline '{idMatch}'");
            string id = Path.GetFileName(matches[0]);
            return GetBaseline(new BaselineId(id));
        }

        public Baseline GetRootBaseline()
        {
            return GetBaseline(GetRootBaselineId());
        }

        public Baseline GetCurrentBaseline()
        {
            return GetBaseline(GetCurrentBaselineId());
        }

        public Baseline GetBaseline(BaselineId id)
        {
            string jsonString = File.ReadAllText(Path.Combine(BaselineDir, id.Filename, "schema.json"));
            var baseline = JsonSerializer.Deserialize<Baseline>(jsonString);
            string hash = Hasher.GetHash(jsonString);
            baseline.Id = new BaselineId(hash);

            if (Path.GetFileName(id.Filename) != baseline.Id.Filename) throw new Exception($"Invalid Baseline '{baseline.Id.Filename}'");

            return baseline;
        }

        public void Stage(string filename)
        {
            string hash = Hasher.GetHash(File.ReadAllBytes(filename));
            File.Copy(filename, Path.Combine(StagingDir, hash));
            File.AppendAllLines(StagingIndexPath, new string[] { $"{hash} {Path.GetFileName(filename)}" });
        }

        public void Unstage(string filename)
        {
            string hash = Hasher.GetHash(File.ReadAllBytes(filename));
            File.Delete(Path.Combine(StagingDir, hash));
            // remove from index
            File.WriteAllLines(StagingIndexPath,
                File.ReadAllLines(StagingIndexPath)
                   .Where(line => !line.StartsWith(hash)));
        }

        public IEnumerable<string> GetStaged()
        {
            return (File.Exists(StagingIndexPath))
                ? File.ReadAllLines(StagingIndexPath).Select(rec => rec.Split(' ')[1])
                : new string[0];
        }

        public IEnumerable<DeltaScript> GetStagedScripts()
        {
            return GetDeltaScripts(StagingIndexPath);
        }

        private IEnumerable<DeltaScript> GetDeltaScripts(string indexPath)
        {
            string baseDir = Directory.GetParent(indexPath).FullName;
            foreach (var rec in File.ReadAllLines(indexPath))
            {
                string[] parts = rec.Split(' ');
                yield return new DeltaScript
                {
                    Name = parts[1],
                    Code = File.ReadAllText(Path.Combine(baseDir, parts[0]))
                };
            }
        }

        public (BaselineId parent, BaselineId @new) Commit(string message, Baseline newBaseline)
        {
            var delta = new Delta
            {
                CommitMessage = message,
                ParentBaselineId = GetCurrentBaselineId(),
                Scripts = GetStagedScripts().ToArray()
            };
            string jsonString = JsonSerializer.Serialize(delta);
            delta.Id = new DeltaId(Hasher.GetHash(jsonString));

            // delta directory
            string deltaDir = Path.Combine(DeltaDir, delta.Id.Filename);
            Directory.CreateDirectory(deltaDir);

            // serialize the object to disk
            delta.ParentBaselineId.Write(Path.Combine(deltaDir, "parent_baseline"));
            File.WriteAllText(Path.Combine(deltaDir, "commit_message"), delta.CommitMessage);
            foreach (var script in delta.Scripts)
            {
                var scriptId = new DeltaScriptId( Hasher.GetHash(script.Code));
                File.WriteAllText(Path.Combine(deltaDir, scriptId.Filename), script.Code);
                File.AppendAllLines(Path.Combine(deltaDir, "index"), new string[] { $"{scriptId.Filename} {script.Name} {scriptId.Hash}" });
            }

            foreach (var staged in Directory.EnumerateFiles(StagingDir))
            {
                File.Delete(staged);
            }

            var newBaselineId = AddBaseline(newBaseline, delta.Id);

            return (parent: delta.ParentBaselineId, @new: newBaselineId);
        }

        public IEnumerable<HistoryItem> GetHistoryBetween(Baseline initialBaseline, Baseline finalBaseline)
        {
            var history = new Stack<HistoryItem>();

            // move backward through history
            var currentId = finalBaseline.Id;
            while (!currentId.Equals(initialBaseline.Id))
            {
                var currentBaseline = GetBaseline(currentId);
                history.Push(currentBaseline);

                string baselineDir = Path.Combine(BaselineDir, currentId.Filename);
                string deltaIdPath = Path.Combine(baselineDir, "delta");
                if (!File.Exists(deltaIdPath))
                    break;
                var deltaId = ObjectId.Read<DeltaId>(deltaIdPath);
                string deltaDir = Path.Combine(DeltaDir, deltaId.Filename);

                var delta = new Delta
                {
                    CommitMessage = File.ReadAllText(Path.Combine(deltaDir, "commit_message")),
                    ParentBaselineId = ObjectId.Read<BaselineId>(Path.Combine(deltaDir, "parent_baseline")),
                    Scripts = GetDeltaScripts(Path.Combine(deltaDir, "index")).ToArray()
                };
                // check delta hash!
                string jsonString = JsonSerializer.Serialize(delta);
                delta.Id = new DeltaId(Hasher.GetHash(jsonString));
                if (!deltaId.Equals(delta.Id)) throw new Exception($"Delta {deltaId} content is tampered");

                history.Push(delta);

                currentId = delta.ParentBaselineId;
            }

            history.Push(initialBaseline);

            return history;
        }

        public IEnumerable<HistoryItem> GetFullHistory()
        {
            // move backward through history
            var currentId = GetCurrentBaselineId();
            while (currentId != null)
            {
                var currentBaseline = GetBaseline(currentId);
                yield return currentBaseline;

                string baselineDir = Path.Combine(BaselineDir, currentId.Filename);
                string deltaIdPath = Path.Combine(baselineDir, "delta");
                if (!File.Exists(deltaIdPath))
                    break;
                var deltaId = ObjectId.Read<DeltaId>(deltaIdPath);
                string deltaDir = Path.Combine(DeltaDir, deltaId.Filename);

                var delta = new Delta
                {
                    CommitMessage = File.ReadAllText(Path.Combine(deltaDir, "commit_message")),
                    ParentBaselineId = ObjectId.Read<BaselineId>(Path.Combine(deltaDir, "parent_baseline")),
                    Scripts = GetDeltaScripts(Path.Combine(deltaDir, "index")).ToArray()
                };
                // check delta hash!
                string jsonString = JsonSerializer.Serialize(delta);
                delta.Id = new DeltaId(Hasher.GetHash(jsonString));
                if (!deltaId.Equals(delta.Id)) throw new Exception($"Delta {deltaId} content is tampered");

                yield return delta;

                currentId = delta.ParentBaselineId;
            }
        }
    }
}
