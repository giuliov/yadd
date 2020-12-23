using System;
using System.Collections.Generic;

namespace yadd.core
{
    public record InformationSchemaTable
    {
        public enum TableType
        {
            BaseTable,
            View,
            Foreign,
            LocalTemporary
        }

        public string Catalog { get; }
        public string Schema { get; }
        public string Name { get; }
        public TableType Type { get; }
        public IEnumerable<InformationSchemaColumn> Columns { get; }

        public InformationSchemaTable(string catalog, string schema, string name, string type, IEnumerable<InformationSchemaColumn> columns)
            => (Catalog, Schema, Name, Type, Columns) = (catalog, schema, name, InformationSchemaTableTypeFromString(type), columns);

        static TableType InformationSchemaTableTypeFromString(string type)
        {
            return type switch
            {
                "BASE TABLE" => TableType.BaseTable,
                "VIEW" => TableType.View,
                "FOREIGN" => TableType.Foreign,
                "LOCAL TEMPORARY" => TableType.LocalTemporary,
                _ => throw new Exception($"Unsupported table type '{type}'")
            };
        }
    }
}