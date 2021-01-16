using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace yadd.core
{
    public class YaddRepoInfo
    {
        const string InfoName = "info";

        static readonly Version RepoFormat = new Version(0, 3);

        private IFileSystem FS { get; init; }
        private string Root { get; init; }
        private string InfoFilePath => FS.Path.Combine(Root, InfoName);

        public YaddRepoInfo(string rootDir, IFileSystem fileSystem)
        {
            FS = fileSystem;
            Root = rootDir;
        }

        public void Write(ServerVersionInfo providerVersion)
        {
            var tomlDoc = new DocumentSyntax()
            {
                Tables =
                    {
                        new TableSyntax("yadd")
                        {
                            Items =
                            {
                                {"version", RepoFormat.ToString()},
                            }
                        },
                        new TableSyntax("provider")
                        {
                            Items =
                            {
                                {"name", providerVersion.Provider },
                                {"version", providerVersion.Version },
                            }
                        }
                    }
            };
            FS.File.WriteAllText(InfoFilePath, tomlDoc.ToString());
        }

        internal bool Read()
        {
            var tomlDoc = Toml.Parse(FS.File.ReadAllText(InfoFilePath), InfoFilePath);
            if (tomlDoc.HasErrors) return false;
            var tomlModel = tomlDoc.ToModel();
            var yaddVersion = new Version((string)((TomlTable)tomlModel["yadd"])["version"]);
            return yaddVersion == RepoFormat;
        }
    }
}
