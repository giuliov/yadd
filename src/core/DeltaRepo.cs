using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace yadd.core
{
    class DeltaRepo
    {
        const string DeltasDirectoryName = "delta";
        const string StagingAreaDirectoryName = "staging";

        private IFileSystem FS { get; init; }
        private string DeltaDir { get; init; }
        private string StagingDir { get; init; }
        protected string StagingIndexPath => FS.Path.Combine(StagingDir, "index");

        public DeltaRepo(IDirectoryInfo repoDir, IFileSystem fileSystem)
        {
            FS = fileSystem;
            DeltaDir = repoDir.CreateSubdirectory(DeltasDirectoryName).FullName;
            StagingDir = repoDir.CreateSubdirectory(StagingAreaDirectoryName).FullName;
        }

        public DeltaRepo(string rootDir, IFileSystem fileSystem)
        {
            FS = fileSystem;
            DeltaDir = FS.Path.Combine(rootDir, DeltasDirectoryName);
            StagingDir = FS.Path.Combine(rootDir, StagingAreaDirectoryName);
        }

        public void StageFile(string filename)
        {
            string hash = Hasher.GetHash(FS.File.ReadAllBytes(filename));
            FS.File.Copy(filename, FS.Path.Combine(StagingDir, hash));
            FS.File.AppendAllLines(StagingIndexPath, new string[] { $"{hash} {FS.Path.GetFileName(filename)}" });
        }

        public void UnstageFile(string filename)
        {
            string hash = Hasher.GetHash(FS.File.ReadAllBytes(filename));
            FS.File.Delete(FS.Path.Combine(StagingDir, hash));
            // remove from index
            FS.File.WriteAllLines(StagingIndexPath,
                FS.File.ReadAllLines(StagingIndexPath)
                   .Where(line => !line.StartsWith(hash)));
        }

        public IEnumerable<string> GetStagedFiles()
        {
            return (FS.File.Exists(StagingIndexPath))
                ? FS.File.ReadAllLines(StagingIndexPath).Select(rec => rec.Split(' ')[1])
                : Array.Empty<string>();
        }

        public IEnumerable<DeltaScript> GetStagedScripts()
        {
            return Delta.GetDeltaScripts(StagingIndexPath, FS);
        }

        public void ClearStagingArea()
        {
            foreach (var staged in FS.Directory.EnumerateFiles(StagingDir))
            {
                FS.File.Delete(staged);
            }
        }

        public Delta AddDelta(string message, BaselineId ParentBaselineId)
        {
            var delta = new Delta
            {
                CommitMessage = message,
                ParentBaselineId = ParentBaselineId,
                Scripts = GetStagedScripts().ToArray()
            };
            delta.SerializeTo(DeltaDir, FS);
            return delta;
        }

        public Delta GetDelta(DeltaId deltaId)
        {
            string deltaDir = FS.Path.Combine(DeltaDir, deltaId.Filename);

            var delta = Delta.DeserializeFrom(deltaDir, FS);

            if (!deltaId.Equals(delta.Id)) throw new Exception($"Delta {deltaId} content was tampered");

            return delta;
        }

    }
}
