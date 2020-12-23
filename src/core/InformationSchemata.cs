using System;
using System.Collections.Generic;

namespace yadd.core
{
    public record InformationSchemata
    {
        public string Catalog { get; }
        public string Schema { get; }
        public string Owner { get; }

        public InformationSchemata(string catalog, string schema, string owner)
            => (Catalog, Schema, Owner) = (catalog, schema, owner);
    }
}