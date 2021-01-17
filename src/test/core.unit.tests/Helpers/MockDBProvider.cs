using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yadd.core;

namespace core.unit.tests
{
    internal class MockDbProvider : GenericProvider
    {
        private MockDbProvider() : base(null,null) { }//forbids

        internal static MockDbProvider Make(IFileSystem FS, string configPath)
        {
            var reader = new DefaultProviderDataReader(FS, configPath);
            return new MockDbProvider(
                reader.Read(), reader.ConfigurationPath);
        }

        private MockDbProvider(string configData, string configPath)
            : base(configData, configPath) { }

        public static string ProvidersToml =>
@"
[mssql]
VersionQuery = ""VersionQuery""
FullVersionQuery = ""FullVersionQuery""
[mssql.infoschema]
Schemata = ""SchemataQuery""
Tables = ""TablesQuery""
TableColumns = ""TableColumnsQuery""
";
        public override string ProviderName => "mssql"; // matches test data

        protected override IDbCommand NewCommand(string query, IDbConnection connection)
        {
            return new MockDbCommand(query, connection);
        }

        protected override IDbConnection NewConnection(string connectionString)
        {
            return new MockDbConnection(connectionString);
        }
    }

    internal class MockDbConnection : IDbConnection
    {
        public MockDbConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public int ConnectionTimeout => throw new NotImplementedException();

        public string Database => throw new NotImplementedException();

        public ConnectionState State => throw new NotImplementedException();

        string IDbConnection.ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            Debug.WriteLine($"MockDbConnection.Close()");
        }

        public IDbCommand CreateCommand()
        {
            return new MockDbCommand();
        }

        public void Dispose()
        {
        }

        public void Open()
        {
            Debug.WriteLine($"MockDbConnection.Open({ConnectionString})");
        }
    }

    internal class MockDbCommand : IDbCommand
    {
        public MockDbCommand()
        {
        }

        public MockDbCommand(string query, IDbConnection connection)
        {
            CommandText = query;
            Connection = connection;
        }

        public IDbConnection Connection { get; }
        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IDataParameterCollection Parameters => throw new NotImplementedException();

        public IDbTransaction Transaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IDbConnection IDbCommand.Connection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public IDbDataParameter CreateParameter()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public int ExecuteNonQuery()
        {
            Debug.WriteLine($"MockDbCommand.ExecuteNonQuery[{CommandText}]()");
            return 42;
        }

        public IDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            Debug.WriteLine($"MockDbCommand.ExecuteReader[{CommandText}]({behavior})");
            var data =  CommandText switch
            {
                "SchemataQuery" => new DataTable
                {
                    TableName = "schemata",
                    Columns = {
                        new DataColumn { ColumnName = "catalog_name" },
                        new DataColumn { ColumnName = "schema_name" },
                        new DataColumn { ColumnName = "schema_owner" },
                    },
                    Rows = { }
                },
                "TablesQuery" => new DataTable
                {
                    TableName = "tables",
                    Columns = {
                        new DataColumn { ColumnName = "table_catalog" },
                        new DataColumn { ColumnName = "table_schema" },
                        new DataColumn { ColumnName = "table_name" },
                        new DataColumn { ColumnName = "table_type" },
                    },
                    Rows = { }
                },
                "TableColumnsQuery" => new DataTable
                {
                    TableName = "table_columns",
                    Columns = {
                        new DataColumn { ColumnName = "table_catalog" },
                        new DataColumn { ColumnName = "table_schema" },
                        new DataColumn { ColumnName = "table_nam" },
                        new DataColumn { ColumnName = "column_name" },
                        new DataColumn { ColumnName = "ordinal_position" },
                        new DataColumn { ColumnName = "column_default" },
                        new DataColumn { ColumnName = "is_nullable" },
                        new DataColumn { ColumnName = "data_type" },
                        new DataColumn { ColumnName = "character_maximum_length" },
                    },
                    Rows = { }
                },
                _ => throw new ArgumentException($"No mock dataset for query {CommandText}"),
            };
            return new MockDataReader(data);
        }

        public object ExecuteScalar()
        {
            Debug.WriteLine($"MockDbCommand.ExecuteScalar[{CommandText}]()");
            return CommandText switch
            {
                "VersionQuery" => "15.0",
                "FullVersionQuery" => "SQL Server 2019",
                _ => throw new ArgumentException($"No mock scalar data for query {CommandText}"),
            };
        }

        public void Prepare()
        {
            throw new NotImplementedException();
        }
    }

    internal class MockDataReader : IDataReader
    {
        private DataTable table;
        private int currentRow = -1;

        public MockDataReader(DataTable table)
        {
            this.table = table;
        }

        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => throw new NotImplementedException();

        public int RecordsAffected => throw new NotImplementedException();

        public int FieldCount => table.Columns.Count;

        public void Close()
        {
            Debug.WriteLine($"MockDataReader.Close()");
        }

        public void Dispose()
        {
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i) => table.Rows[currentRow].Field<Int32>(i);

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i) => table.Columns[i].ColumnName;

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i) => table.Rows[currentRow].Field<string>(i);

        public object GetValue(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i) => table.Rows[currentRow].IsNull(i);

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read() => currentRow++ > table.Rows.Count;
    }
}
