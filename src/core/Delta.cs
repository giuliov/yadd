using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;

namespace yadd.core
{
    public class DeltaScript
    {
        public string Name { get; internal set; }
        public string Code { get; internal set; }
    }

    public class Delta
    {
        public DeltaId Id { get; internal set; }
        public string CommitMessage { get; internal set; }
        public DeltaScript[] Scripts { get; internal set; }
        public BaselineId ParentBaselineId { get; internal set; }

        public DeltaId SerializeTo(string deltasDir, IFileSystem FS)
        {
            string jsonString = JsonSerializer.Serialize(this);
            Id = new DeltaId(Hasher.GetHash(jsonString));

            // this directory
            string deltaDir = FS.Path.Combine(deltasDir, Id.Filename);
            FS.Directory.CreateDirectory(deltaDir);

            // serialize the object to disk
            this.ParentBaselineId.Write(FS.Path.Combine(deltaDir, "parent_baseline"), FS);
            FS.File.WriteAllText(FS.Path.Combine(deltaDir, "commit_message"), CommitMessage);
            foreach (var script in Scripts)
            {
                var scriptId = new DeltaScriptId(Hasher.GetHash(script.Code));
                FS.File.WriteAllText(FS.Path.Combine(deltaDir, scriptId.Filename), script.Code);
                FS.File.AppendAllLines(FS.Path.Combine(deltaDir, "index"), new string[] { $"{scriptId.Filename} {script.Name} {scriptId.Hash}" });
            }

            return Id;
        }

        public static Delta DeserializeFrom(string deltaDir, IFileSystem FS)
        {
            var delta = new Delta
            {
                CommitMessage = FS.File.ReadAllText(FS.Path.Combine(deltaDir, "commit_message")),
                ParentBaselineId = ObjectId.Read<BaselineId>(FS.Path.Combine(deltaDir, "parent_baseline"), FS),
                Scripts = GetDeltaScripts(FS.Path.Combine(deltaDir, "index"), FS).ToArray()
            };
            // check delta hash!
            string jsonString = JsonSerializer.Serialize(delta);
            delta.Id = new DeltaId(Hasher.GetHash(jsonString));

            return delta;
        }

        public static IEnumerable<DeltaScript> GetDeltaScripts(string indexPath, IFileSystem FS)
        {
            if (!FS.File.Exists(indexPath)) yield break;

            string baseDir = FS.Directory.GetParent(indexPath).FullName;
            foreach (var rec in FS.File.ReadAllLines(indexPath))
            {
                string[] parts = rec.Split(' ');
                yield return new DeltaScript
                {
                    Name = parts[1],
                    Code = FS.File.ReadAllText(FS.Path.Combine(baseDir, parts[0]))
                };
            }
        }
    }
}
