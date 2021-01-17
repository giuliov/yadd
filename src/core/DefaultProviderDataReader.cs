using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace yadd.core
{
    public class DefaultProviderDataReader
    {
        private IFileSystem FS { get; }
        public string ConfigurationPath { get; }

        public DefaultProviderDataReader(IFileSystem fs, string configPath = "providers.toml") {
            this.FS = fs;
            this.ConfigurationPath = FindConfigurationFile(configPath);
        }

        private string FindConfigurationFile(string configPath)
        {
            if (string.IsNullOrEmpty(configPath)) throw new Exception($"Invalid {configPath}");

            string probe = FS.Path.GetFullPath(configPath);
            if (FS.File.Exists(probe)) return probe;

            string name = FS.Path.GetFileName(configPath);
            probe = FS.Path.Combine(
                FS.Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly()?.Location),
                name);
            if (FS.File.Exists(probe)) return probe;

            probe = FS.Path.Combine(
                FS.Path.GetDirectoryName(
                    Assembly.GetCallingAssembly()?.Location),
                name);
            if (FS.File.Exists(probe)) return probe;

            probe = FS.Path.Combine(
                FS.Path.GetDirectoryName(
                    Assembly.GetEntryAssembly()?.Location),
                name);
            if (FS.File.Exists(probe)) return probe;

            throw new Exception($"Cannot find {configPath}");
        }

        public string Read() => FS.File.ReadAllText(ConfigurationPath);
    }
}
