using Semver;
using System;

namespace yadd.core
{
    public interface IProvider
    {
        SemVersion ProviderVersion { get; }
        ServerVersionInfo GetServerVersion();

        IDataDefinition DataDefinition { get; }
        IScriptRunner ScriptRunner { get; }
    }
}
