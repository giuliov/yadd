using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace yadd.core
{
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
        protected string StagingIndexPath => Path.Combine(StagingDir, "index.txt");

        public static Repository Init(Baseline baseline)
        {
            var repoDir = Directory.CreateDirectory(RootName);
            File.WriteAllText(Path.Combine(repoDir.FullName, InfoName), $"{RepoFormat} yadd");
            var repo = new Repository {
                Root = repoDir.FullName,
                // create initial structure
                BaselineDir = repoDir.CreateSubdirectory("baseline").FullName,
                DeltaDir = repoDir.CreateSubdirectory("delta").FullName,
                StagingDir = repoDir.CreateSubdirectory("staging").FullName
            };
            repo.AddBaseline(baseline);
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

        public Baseline GetBaseline(string idMatch)
        {
            var matches = Directory.GetDirectories(BaselineDir, idMatch + "*");
            if (matches.Length != 1) throw new Exception($"Cannot find Baseline '{idMatch}'");

            string jsonString = File.ReadAllText(Path.Combine(BaselineDir, matches[0], "schema.json"));
            var baseline = JsonSerializer.Deserialize<Baseline>(jsonString);
            string hash = Hasher.GetHash(jsonString);
            baseline.Id = new BaselineId(hash);

            if (Path.GetFileName(matches[0]) != baseline.Id.Filename) throw new Exception($"Invalid Baseline '{baseline.Id.Filename}'");

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

        public IEnumerable<(string name, string code)> GetStagedScripts()
        {
            return GetDeltaScripts(StagingIndexPath);
        }

        private IEnumerable<(string name, string code)> GetDeltaScripts(string indexPath)
        {
            string baseDir = Directory.GetParent(indexPath).FullName;
            foreach (var rec in File.ReadAllLines(indexPath))
            {
                string[] parts = rec.Split(' ');
                yield return (parts[1], File.ReadAllText(Path.Combine(baseDir, parts[0])));
            }
        }

        public void Commit(string message, Baseline newBaseline)
        {
            var workDir = Directory.CreateDirectory(Path.Combine(DeltaDir, "tmp"));

            var steps = new List<string>();
            foreach (var full in Directory.EnumerateFiles(StagingDir))
            {
                string name = Path.GetFileName(full);
                steps.Add(name);
                string target = Path.Combine(workDir.FullName, name);
                File.Move(full, target);
            }
            var parentId = GetCurrentBaselineId();
            parentId.Write(Path.Combine(workDir.FullName, "parent_baseline"));
            steps.Add(parentId.Hash);
            File.WriteAllText(Path.Combine(workDir.FullName, "commit_message"), message);
            steps.Add(message);

            string hash = Hasher.GetHash(string.Join('\n', steps));
            var deltaId = new DeltaId(hash);
            workDir.MoveTo(Path.Combine(DeltaDir, deltaId.Filename));

            var newBaselineId = AddBaseline(newBaseline, deltaId);
        }

        public IEnumerable<Delta> GetDeltas(Baseline initialBaseline)
        {
            var deltas = new Stack<Delta>();

            var currentId = GetCurrentBaselineId();
            while (!currentId.Equals(initialBaseline.Id))
            {
                string baselineDir = Path.Combine(BaselineDir, currentId.Filename);
                string deltaIdPath = Path.Combine(baselineDir, "delta");
                if (!File.Exists(deltaIdPath))
                    break;
                var deltaId = ObjectId.Read<DeltaId>(deltaIdPath);
                string deltaDir = Path.Combine(DeltaDir, deltaId.Filename);
                var delta = new Delta {
                    Id = deltaId,
                    CommitMessage = File.ReadAllText(Path.Combine(deltaDir, "commit_message")),
                    ParentBaselineId = ObjectId.Read<BaselineId>(Path.Combine(deltaDir, "parent_baseline")),
                    Scripts = new List<DeltaScript>()
                };
                foreach (var item in GetDeltaScripts(Path.Combine(deltaDir, "index.txt")))
                {
                    delta.Scripts.Add(new DeltaScript { Name = item.name, Code = item.code });
                }

                deltas.Push(delta);

                currentId = delta.ParentBaselineId;
            }
            return deltas;
        }
    }
}
