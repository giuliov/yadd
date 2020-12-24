namespace yadd.core
{
    public interface IProvider
    {
        IDataDefinition DataDefinition { get; }
        IScriptRunner ScriptRunner { get; }
    }
}
