using System;

namespace yadd.core
{
    public record ServerVersionInfo
    {
        public string Provider { get; init; }
        public string Version { get; init; }
        public string FullVersion { get; init; }
    }
}
