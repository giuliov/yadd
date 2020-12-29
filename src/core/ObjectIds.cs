using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace yadd.core
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public abstract class ObjectId
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public string Hash { get; init; }
        public string Filename => Hash.Substring(0, 38);
        public string Displayname => Hash.Substring(0, 8);

        protected ObjectId() { }

        public ObjectId(string hash)
        {
            Hash = hash;
        }

        public override bool Equals(object obj)
        {
            if (!obj.GetType().IsSubclassOf(typeof(ObjectId)))
                return false;
            return this.Hash == ((ObjectId)obj).Hash;
        }

        public void Write(string path)
        {
            File.WriteAllText(path, Hash);
        }

        public static T Read<T>(string path) where T : ObjectId
        {
            return File.Exists(path)
                ? (T)Activator.CreateInstance(typeof(T), File.ReadAllText(path))
                : null;
        }
    }

    public class BaselineId : ObjectId
    {
        public BaselineId(string hash) : base(hash) { }
    }

    public class DeltaId : ObjectId
    {
        public DeltaId(string hash) : base(hash) { }
    }

    public class DeltaScriptId : ObjectId
    {
        public DeltaScriptId(string hash) : base(hash) { }
    }
}
