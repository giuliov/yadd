using System;
using System.IO.Abstractions;

namespace yadd.core
{
    public class BaselineRepo
    {
        const string BaselinesDirectoryName = "baseline";

        private IFileSystem FS { get; init; }
        private string Root { get; init; }
        private string BaselineDir { get; init; }

        public BaselineRepo(IDirectoryInfo repoDir, IFileSystem fileSystem)
        {
            FS = fileSystem;
            Root = repoDir.FullName;
            BaselineDir = repoDir.CreateSubdirectory(BaselinesDirectoryName).FullName;
        }

        public BaselineRepo(string rootDir, IFileSystem fileSystem)
        {
            FS = fileSystem;
            Root = rootDir;
            BaselineDir = FS.Path.Combine(rootDir, BaselinesDirectoryName);
        }


        internal void AddRootBaseline(Baseline baseline, References references)
        {
            baseline.ParentId = null;
            baseline.DeltaId = null;

            // sanity check
            if (references.GetRootBaselineId() != null) throw new Exception($"Repository is alredy initialized");

            var id = baseline.SerializeTo(BaselineDir, FS);

            references.SetRootBaselineId(id);
        }

        public BaselineId AddBaseline(Baseline baseline, DeltaId deltaId, BaselineId parentId)
        {
            baseline.ParentId = parentId;
            baseline.DeltaId = deltaId;

            var id = baseline.SerializeTo(BaselineDir, FS);

            baseline.Id = id;

            return id;
        }

        public Baseline GetBaseline(BaselineId id)
        {
            var baseline = Baseline.DeserializeFrom(FS.Path.Combine(BaselineDir, id.Filename), FS);

            if (!id.Equals(baseline.Id)) throw new Exception($"Invalid Baseline '{baseline.Id.Filename}'");

            return baseline;
        }

        public (bool found, BaselineId id) FindMatch(Baseline baseline)
        {
            var core = baseline.GetCore();

            foreach (var dir in FS.Directory.EnumerateDirectories(BaselineDir))
            {
                string schema_hash = FS.File.ReadAllText(FS.Path.Combine(dir, "schema_hash"));
                if (schema_hash == core.hash)
                {
                    string meta = FS.File.ReadAllText(FS.Path.Combine(dir, "meta"));
                    var id = new BaselineId(Hasher.GetHash(meta));
                    return (found: true, id: id);
                }
            }

            return (found: false, null);
        }

        public DeltaId GetDelta(BaselineId id)
        {
            string baselineDir = FS.Path.Combine(BaselineDir, id.Filename);
            string deltaIdPath = FS.Path.Combine(baselineDir, "delta");
            if (!FS.File.Exists(deltaIdPath))
                return null;
            var deltaId = ObjectId.Read<DeltaId>(deltaIdPath, FS);
            return deltaId;
        }
    }
}
