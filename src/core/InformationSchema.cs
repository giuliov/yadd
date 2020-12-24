using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace yadd.core
{
    public class InformationSchema
    {
        public InformationSchemata[] Schemata { get; set; }
        public InformationSchemaTable[] Tables { get; set; }
    }
}