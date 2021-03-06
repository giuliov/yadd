﻿using System;
using System.IO.Abstractions;
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
        public DateTimeOffset Timestamp { get; internal set; } = DateTimeOffset.Now;
        public ServerVersionInfo ServerInfo { get; set; }
        public string ProviderConfigurationData { get; set; }
        public string Data { get; set; }

        private const string MetadataVersion = "0.3";

        public (string data, string hash) GetCore()
        {
            string schema_hash = Hasher.GetHash(Data);
            return (data: Data, hash: schema_hash);
        }

        public BaselineId SerializeTo(string baselinesDir, IFileSystem FS)
        {
            string baselineDir = FS.Path.Combine(baselinesDir, "tmp");
            FS.Directory.CreateDirectory(baselineDir);

            var core = GetCore();
            FS.File.WriteAllText(FS.Path.Combine(baselineDir, "schema_data"), core.data);
            FS.File.WriteAllText(FS.Path.Combine(baselineDir, "schema_hash"), core.hash);
            FS.File.WriteAllText(FS.Path.Combine(baselineDir, "provider_configuration"), ProviderConfigurationData);
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
                            }
                        },
                        new TableSyntax(new KeySyntax("baseline", "serverinfo"))
                        {
                            Items =
                            {
                                {"provider", ServerInfo.Provider },
                                {"version", ServerInfo.Version },
                                {"fullversion", ServerInfo.FullVersion }
                            }
                        },
                    }
            };

            FS.File.WriteAllText(FS.Path.Combine(baselineDir, "meta"), tomlDoc.ToString());
            string meta = FS.File.ReadAllText(FS.Path.Combine(baselineDir, "meta"));
            var id = new BaselineId(Hasher.GetHash(meta));
            FS.File.WriteAllText(FS.Path.Combine(baselineDir, "id"), id.Hash);

            if (ParentId != null) ParentId.Write(FS.Path.Combine(baselineDir, "parent_baseline"), FS);
            if (DeltaId != null) DeltaId.Write(FS.Path.Combine(baselineDir, "delta"), FS);

            FS.Directory.Move(baselineDir, FS.Path.Combine(baselinesDir, id.Filename));

            return id;
        }

        public static Baseline DeserializeFrom(string baselineDir, IFileSystem FS)
        {
            var b = new Baseline();

            string jsonString = FS.File.ReadAllText(FS.Path.Combine(baselineDir, "schema_data"));
            string hash = Hasher.GetHash(jsonString);
            string schema_hash = FS.File.ReadAllText(FS.Path.Combine(baselineDir, "schema_hash"));
            if (hash != schema_hash) ThrowInvalidBaseline();

            b.Data = jsonString;
            string metaPath = FS.Path.Combine(baselineDir, "meta");
            var metaDoc = Toml.Parse(FS.File.ReadAllText(metaPath), metaPath);
            if (metaDoc.HasErrors) ThrowInvalidBaseline();

            var metaToml = metaDoc.ToModel();
            var metadataVersion = (string)((TomlTable)metaToml["meta"])["version"];

            if (metadataVersion != MetadataVersion) ThrowInvalidBaseline();
            var baselineTable = (TomlTable)metaToml["baseline"];

            b.Timestamp = DateTimeOffset.Parse((string)baselineTable["timestamp"]);
            string meta_schema_hash = (string)baselineTable["schema_hash"];
            if (meta_schema_hash != schema_hash) ThrowInvalidBaseline();
            var serverInfoTable = (TomlTable)baselineTable["serverinfo"];
            b.ServerInfo = new ServerVersionInfo
            {
                Provider = (string)serverInfoTable["provider"],
                Version = (string)serverInfoTable["version"],
                FullVersion = (string)serverInfoTable["fullversion"]
            };
            b.ProviderConfigurationData = FS.File.ReadAllText(FS.Path.Combine(baselineDir, "provider_configuration"));
            string provider_configuration_hash = Hasher.GetHash(b.ProviderConfigurationData);
            string meta_provider_configuration_hash = (string)baselineTable["provider_configuration_hash"];
            if (meta_provider_configuration_hash != provider_configuration_hash) ThrowInvalidBaseline();

            if (FS.File.Exists(FS.Path.Combine(baselineDir, "parent_baseline")))
            {
                b.ParentId = ObjectId.Read<BaselineId>(FS.Path.Combine(baselineDir, "parent_baseline"), FS);
            }
            if (FS.File.Exists(FS.Path.Combine(baselineDir, "delta")))
            {
                b.DeltaId = ObjectId.Read<DeltaId>(FS.Path.Combine(baselineDir, "delta"), FS);
            }
            string metastring = FS.File.ReadAllText(metaPath);
            b.Id = new BaselineId(Hasher.GetHash(metastring));

            return b;

            static void ThrowInvalidBaseline()
            {
                throw new Exception("Invalid baseline: repo is corrupt");
            }
        }
    }
}
