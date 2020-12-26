using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace yadd.core
{
    public class Baseline
    {
        [JsonIgnore]
        public BaselineId Id { get; set; }
        [JsonIgnore]
        public BaselineId ParentId { get; set; }
        [JsonIgnore]
        public DeltaId DeltaId { get; set; }
        public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.Now;
        public ServerVersionInfo ServerInfo { get; set; }
        public InformationSchema InformationSchema { get; set; }

        private const string MetadataVersion = "0.1 meta";

        public (string data, string hash) GetCore()
        {
            string jsonString = JsonSerializer.Serialize(InformationSchema);
            string schema_hash = Hasher.GetHash(jsonString);
            return (data: jsonString, hash: schema_hash);
        }

        public BaselineId SerializeTo(string baselinesDir)
        {
            string baselineDir = Path.Combine(baselinesDir, "tmp");
            Directory.CreateDirectory(baselineDir);

            var core = GetCore();
            File.WriteAllText(Path.Combine(baselineDir, "schema.json"), core.data);
            File.WriteAllText(Path.Combine(baselineDir, "schema_hash"), core.hash);
            File.WriteAllLines(Path.Combine(baselineDir, "meta"), new string[] {
                MetadataVersion,
                Timestamp.ToString("O"),
                core.hash,
                ServerInfo.Provider,
                ServerInfo.Version,
                ServerInfo.FullVersion
            });
            string meta = File.ReadAllText(Path.Combine(baselineDir, "meta"));
            var id = new BaselineId(Hasher.GetHash(meta));

            if (ParentId != null) ParentId.Write(Path.Combine(baselineDir, "parent_baseline"));
            if (DeltaId != null) DeltaId.Write(Path.Combine(baselineDir, "delta"));

            Directory.Move(baselineDir, Path.Combine(baselinesDir, id.Filename));

            return id;
        }

        public static Baseline DeserializeFrom(string baselineDir)
        {
            var b = new Baseline();

            string jsonString = File.ReadAllText(Path.Combine(baselineDir, "schema.json"));
            string hash = Hasher.GetHash(jsonString);
            string schema_hash = File.ReadAllText(Path.Combine(baselineDir, "schema_hash"));
            if (hash != schema_hash) ThrowInvalidBaseline();

            b.InformationSchema = JsonSerializer.Deserialize<InformationSchema>(jsonString);
            var meta = File.ReadAllLines(Path.Combine(baselineDir, "meta"));
            if (meta[0] != MetadataVersion) ThrowInvalidBaseline();
            b.Timestamp = DateTimeOffset.Parse(meta[1]);
            if (meta[2] != schema_hash) ThrowInvalidBaseline();
            b.ServerInfo = new ServerVersionInfo
            {
                Provider = meta[3],
                Version = meta[4],
                FullVersion = meta[5]
            };
            if (File.Exists(Path.Combine(baselineDir, "parent_baseline")))
            {
                b.ParentId = ObjectId.Read<BaselineId>(Path.Combine(baselineDir, "parent_baseline"));
            }
            if (File.Exists(Path.Combine(baselineDir, "delta")))
            {
                b.DeltaId = ObjectId.Read<DeltaId>(Path.Combine(baselineDir, "delta"));
            }
            string metastring = File.ReadAllText(Path.Combine(baselineDir, "meta"));
            b.Id = new BaselineId(Hasher.GetHash(metastring));

            return b;

            static void ThrowInvalidBaseline()
            {
                throw new Exception("Invalid baseline: repo is corrupt");
            }
        }
    }
}
