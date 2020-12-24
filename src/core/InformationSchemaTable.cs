using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace yadd.core
{
    public class InformationSchemaTable
    {
        public enum TableType
        {
            BaseTable,
            View,
            Foreign,
            LocalTemporary
        }

        public string Catalog { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public TableType Type { get; set; }
        public InformationSchemaColumn[] Columns { get; set; }
    }
}