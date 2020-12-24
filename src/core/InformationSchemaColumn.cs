using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace yadd.core
{
    public class InformationSchemaColumn
    {
        public string Name { get; set; }
        public int Position { get; set; } // Ordinal position of the column within the table(count starts at 1)
        public string Default { get; set; }
        public bool Nullable{ get; set; }
        public string DataType { get; set; }
        public int MaximumLength { get; set; }
    }
}
