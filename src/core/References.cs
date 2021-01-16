using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace yadd.core
{
    public class References
    {
        const string BaselinesDirectoryName = "baseline";// HACK duplicate
        const string ReferenceDirectoryName = "reference";
        const string BranchesDirectoryName = "branches";
        const string DefaultBranchName = "main";
        const string TagsDirectoryName = "tags";

        private IFileSystem FS { get; }
        private string Root { get; init; }
        private string BaselineDir => FS.Path.Combine(Root, BaselinesDirectoryName);

        private string ReferenceDir { get; init; }
        private string RootBaselinePath => FS.Path.Combine(ReferenceDir, "root_baseline");

        protected string CurrentBaselinePath => FS.Path.Combine(Root, "current_baseline");
        protected string BranchesDir { get; init; }
        protected string TagsDir { get; init; }

        public References(IDirectoryInfo repoDir, IFileSystem fileSystem)
        {
            FS = fileSystem;
            Root = repoDir.FullName;
            var refDir = repoDir.CreateSubdirectory(ReferenceDirectoryName);
            ReferenceDir = refDir.FullName;
            BranchesDir = refDir.CreateSubdirectory(BranchesDirectoryName).FullName;
            TagsDir = refDir.CreateSubdirectory(TagsDirectoryName).FullName;
        }

        public References(string rootDir, IFileSystem fileSystem)
        {
            FS = fileSystem;
            Root = rootDir;
            ReferenceDir = FS.Path.Combine(rootDir, ReferenceDirectoryName);
            BranchesDir = FS.Path.Combine(ReferenceDir, BranchesDirectoryName);
            TagsDir = FS.Path.Combine(ReferenceDir, TagsDirectoryName);
        }

        public void SetRootBaselineId(BaselineId rootBaselineId)
        {
            rootBaselineId.Write(RootBaselinePath, FS);
            rootBaselineId.Write(FS.Path.Combine(BranchesDir, DefaultBranchName), FS);
            FS.File.WriteAllText(CurrentBaselinePath, $":{DefaultBranchName}");
        }

        public BaselineId GetRootBaselineId()
        {
            return ObjectId.Read<BaselineId>(RootBaselinePath, FS);
        }

        public BaselineId GetCurrentBaselineId()
        {
            string current = FS.File.ReadAllText(CurrentBaselinePath);
            if (current.StartsWith(':'))
            {
                // indirect ref via branch
                current = current.Substring(1);
                return ObjectId.Read<BaselineId>(FS.Path.Combine(BranchesDir, current), FS);
            }
            else
            {
                // direct ref
                return ObjectId.Read<BaselineId>(CurrentBaselinePath, FS);
            }
        }

        internal void SetCurrent(BaselineId newBaselineId)
        {
            string current = FS.File.ReadAllText(CurrentBaselinePath);
            if (current.StartsWith(':'))
            {
                // indirect ref via branch
                current = current.Substring(1);
                newBaselineId.Write(FS.Path.Combine(BranchesDir, current), FS);
            }
            else
            {
                // direct ref
                newBaselineId.Write(CurrentBaselinePath, FS);
            }

        }

        internal IEnumerable<(BaselineRef name, bool current)> GetAllBranches()
        {
            string current = FS.File.ReadAllText(CurrentBaselinePath).Substring(1);
            foreach (var item in FS.Directory.GetFiles(BranchesDir))
            {
                string branchName = FS.Path.GetFileName(item);
                yield return (new BaselineRef(branchName), current == branchName);
            }
        }

        public void SwitchToBranch(BaselineRef branch)
        {
            string branchPointerPath = FS.Path.Combine(BranchesDir, branch.ToString());
            var id = FS.File.Exists(branchPointerPath)
                // existing branch
                ? new BaselineId(FS.File.ReadAllText(branchPointerPath))
                // new branch
                : GetCurrentBaselineId();
            FS.File.WriteAllText(CurrentBaselinePath, $":{branch}");
            SetCurrent(id);
        }

        internal void DeleteBranch(BaselineRef branch)
        {
            string current = FS.File.ReadAllText(CurrentBaselinePath);
            if (current == $":{branch}") throw new Exception("Cannot remove current branch");
            FS.File.Delete(FS.Path.Combine(BranchesDir, branch.ToString()));
        }

        public void AddTag(BaselineRef tag, BaselineId target)
        {
            target.Write(FS.Path.Combine(TagsDir, tag.ToString()), FS);
        }

        public void RemoveTag(BaselineRef tag)
        {
            string tagFile = FS.Path.Combine(TagsDir, tag.ToString());
            if (!FS.File.Exists(tagFile)) throw new Exception($"Tag '{tag}' not found");
            FS.File.Delete(tagFile);
        }

        public BaselineId Resolve(BaselineRef bref)
        {
            // is it a short name for a baseline?
            var matches = FS.Directory.GetDirectories(BaselineDir, bref.DirectoryMatchingPattern);
            if (matches.Length == 1)
            {
                // yes
                string id = FS.File.ReadAllText(FS.Path.Combine(matches[0], "id"));
                return new BaselineId(id);
            }
            else if (matches.Length > 1)
            {
                // uh oh
                throw new Exception($"Ambigous specification '{bref}'");
            }
            // TODO should we include branches????
            else
            {
                // see if matches a tag
                string tagPath = FS.Path.Combine(TagsDir, bref.ToString());
                return ObjectId.Read<BaselineId>(tagPath, FS);
            }
        }

        internal IEnumerable<(BaselineRef tag, BaselineId id)> GetAllTags()
        {
            foreach (var item in FS.Directory.GetFiles(TagsDir))
            {
                yield return (new BaselineRef(FS.Path.GetFileName(item)), new BaselineId(FS.File.ReadAllText(item)));
            }
        }
    }
}
