using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

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
        public string ProviderConfigurationData { get; set; }
        public string Data { get; set; }

        private const string MetadataVersion = "0.3";

        public (string data, string hash) GetCore()
        {
            string schema_hash = Hasher.GetHash(Data);
            return (data: Data, hash: schema_hash);
        }

        public BaselineId SerializeTo(string baselinesDir)
        {
            string baselineDir = Path.Combine(baselinesDir, "tmp");
            Directory.CreateDirectory(baselineDir);

            var core = GetCore();
            File.WriteAllText(Path.Combine(baselineDir, "schema_data"), core.data);
            File.WriteAllText(Path.Combine(baselineDir, "schema_hash"), core.hash);
            File.WriteAllText(Path.Combine(baselineDir, "provider_configuration"), ProviderConfigurationData);
            string provider_configuration_hash = Hasher.GetHash(ProviderConfigurationData);

            var tomlDoc = new DocumentSyntax()
            {
                Tables =
                    {
                        new TableSyntax("meta")
                        {
                            Items =
                            {
                                {"version", MetadataVersion},
                            }
                        },
                        new TableSyntax("baseline")
                        {
                            Items =
                            {
                                {"timestamp", Timestamp.ToString("O") },
                                {"schema_hash", core.hash },
                                {"provider_configuration_hash", provider_configuration_hash },
                                {"serverinfo.provider", ServerInfo.Provider },
                                {"serverinfo.version", ServerInfo.Version },
                                {"serverinfo.fullversion", ServerInfo.FullVersion }
                            }
                        }
                    }
            };

            File.WriteAllText(Path.Combine(baselineDir, "meta"), tomlDoc.ToString());
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

            string jsonString = File.ReadAllText(Path.Combine(baselineDir, "schema_data"));
            string hash = Hasher.GetHash(jsonString);
            string schema_hash = File.ReadAllText(Path.Combine(baselineDir, "schema_hash"));
            if (hash != schema_hash) ThrowInvalidBaseline();

            b.Data = jsonString;
            string metaPath = Path.Combine(baselineDir, "meta");
            var metaDoc = Toml.Parse(File.ReadAllText(metaPath), metaPath);
            if (metaDoc.HasErrors) ThrowInvalidBaseline();

            var metaToml = metaDoc.ToModel();
            var metadataVersion = (string)((TomlTable)metaToml["meta"])["version"];

            if (metadataVersion != MetadataVersion) ThrowInvalidBaseline();
            var baselineTable = (TomlTable)metaToml["baseline"];

            b.Timestamp = DateTimeOffset.Parse((string)baselineTable["timestamp"]);
            string meta_schema_hash = (string)baselineTable["schema_hash"];
            if (meta_schema_hash != schema_hash) ThrowInvalidBaseline();
            //var serverInfoTable = (TomlTable)baselineTable["serverinfo"];
            b.ServerInfo = new ServerVersionInfo
            {
                Provider = (string)baselineTable["serverinfo.provider"],
                Version = (string)baselineTable["serverinfo.version"],
                FullVersion = (string)baselineTable["serverinfo.fullversion"]
            };
            b.ProviderConfigurationData = File.ReadAllText(Path.Combine(baselineDir, "provider_configuration"));
            string provider_configuration_hash = Hasher.GetHash(b.ProviderConfigurationData);
            string meta_provider_configuration_hash = (string)baselineTable["provider_configuration_hash"];
            if (meta_provider_configuration_hash != provider_configuration_hash) ThrowInvalidBaseline();

            if (File.Exists(Path.Combine(baselineDir, "parent_baseline")))
            {
                b.ParentId = ObjectId.Read<BaselineId>(Path.Combine(baselineDir, "parent_baseline"));
            }
            if (File.Exists(Path.Combine(baselineDir, "delta")))
            {
                b.DeltaId = ObjectId.Read<DeltaId>(Path.Combine(baselineDir, "delta"));
            }
            string metastring = File.ReadAllText(metaPath);
            b.Id = new BaselineId(Hasher.GetHash(metastring));

            return b;

            static void ThrowInvalidBaseline()
            {
                throw new Exception("Invalid baseline: repo is corrupt");
            }
        }
    }
}
