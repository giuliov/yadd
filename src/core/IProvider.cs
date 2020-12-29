using Semver;
using System;

namespace yadd.core
{
    public interface IProvider
    {
        SemVersion ProviderVersion { get; }
        ServerVersionInfo GetServerVersion();
        string ProviderConfigurationData { get; }

        IDataDefinition DataDefinition { get; }
        IScriptRunner ScriptRunner { get; }
    }
}
