using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yadd.core
{
    public record InformationSchemaColumn
    {
        public string Name { get; }
        public int Position { get; } // Ordinal position of the column within the table(count starts at 1)
        public string Default { get; }
        public bool Nullable{ get; }
        public string DataType { get; }
        public int MaximumLength { get; }

        public InformationSchemaColumn(string name, int position, string @default, string nullable, string dataType, int maximumLength)
            => (Name, Position , Default , Nullable , DataType, MaximumLength) = (name, position, @default, BoolFromYesOrNoString(nullable), dataType, maximumLength);

        static bool BoolFromYesOrNoString(string yesOrNo)
        {
            return yesOrNo.ToLower() == "yes";
        }
    }
}
