using System.Collections.Generic;

namespace yadd.core
{
    public abstract class InformationSchema
    {
        public IEnumerable<InformationSchemata> Schemata { get; protected set; }
        public IEnumerable<InformationSchemaTable> Tables { get; protected set; }
    }
}