using System;
using System.Data;
using System.Globalization;
using System.Text;

namespace yadd.core
{
    public static class Serializer
    {
        public static string Serialize(IDataReader reader, string queryName)
        {
            var sb = new StringBuilder();
            sb.Append($"{queryName}");
            WriteDataReader(sb, reader);
            return sb.ToString();
        }

        public static void WriteDataReader(StringBuilder sb, IDataReader reader)
        {
            if (reader == null || reader.FieldCount == 0)
            {
                sb.Append("null");
                return;
            }

            sb.Append("(");
            for (int i = 0; i < reader.FieldCount; i++)
            {
                sb.Append(reader.GetName(i));
                sb.Append(',');
            }
            // strip off trailing comma
            if (reader.FieldCount > 0)
                StripComma(sb);
            sb.Append(")[\r\n");

            int rowCount = 0;

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    WriteValue(sb, reader[i]);
                    sb.Append(',');
                }
                // strip off trailing comma
                if (reader.FieldCount > 0)
                    StripComma(sb);
                sb.Append(";\r\n");

                rowCount++;
            }
            // strip off trailing comma
            if (rowCount > 0)
                StripComma(sb);
            sb.Append("]\r\n");
        }

        private static void StripComma(StringBuilder sb)
        {
            sb.Length--;  // ,
        }

        // This serialization code is based on Jason Diamonds JSON parsing 
        // routines part of MyAjax.NET (aka Anthem).
        /// <summary>
        /// Serialization routine that takes any value and serializes
        /// it into JSON. 
        /// 
        /// Date formatting follows Microsoft ASP.NET AJAX format which
        /// represents dates as strings in the format of: "\/Date(231231231)\/"
        ///
        /// This code is based originally on Jason Diamond's JSON code
        /// in Anthem.NET although heavy modifications have been made.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="val"></param>
        public static void WriteValue(StringBuilder sb, object val)
        {
            if (val == null || val == System.DBNull.Value)
            {
                sb.Append("null");
            }
            else if (val is string)
            {
                WriteString(sb, (string)val);
            }
            else if (val is byte[]) // added by Radenko
            {
                WriteString(sb, Convert.ToBase64String((byte[])val));
            }
            else if (val is bool)
            {
                sb.Append(val.ToString().ToLower());
            }
            else if (val is long ||
                     val is int ||
                     val is short ||
                     val is byte
                )
            {
                sb.Append(val);
            }
            else if (val is decimal)
            {
                sb.Append(((decimal)val).ToString(CultureInfo.InvariantCulture.NumberFormat));
            }
            else if (val is float)
            {
                sb.Append(((float)val).ToString(CultureInfo.InvariantCulture.NumberFormat));
            }
            else if (val is double)
            {
                sb.Append(((double)val).ToString(CultureInfo.InvariantCulture.NumberFormat));
            }
            else if (val is Single)
            {
                sb.Append(((Single)val).ToString(CultureInfo.InvariantCulture.NumberFormat));
            }
            else if (val.GetType().IsEnum)
            {
                sb.Append((int)val);
            }
            else if (val is DateTime)
            {
                sb.Append(((DateTime)val).ToString("s"));
            }
            else if (val is DateTimeOffset)
            {
                sb.Append(((DateTimeOffset)val).ToString("s"));
            }
            //else if (val is DataSet)
            //{
            //    this.WriteDataSet(sb, val as DataSet);
            //}
            //else if (val is DataTable)
            //{
            //    this.WriteDataTable(sb, val as DataTable);
            //}
            //else if (val is DataRow)
            //{
            //    this.WriteDataRow(sb, val as DataRow);
            //}
            //else if (val is IDataReader)
            //{
            //    this.WriteDataReader(sb, val as IDataReader);
            //}
            //else if (val is IDictionary)
            //{
            //    this.WriteDictionary(sb, val as IDictionary);
            //}
            //else if (val is IEnumerable)
            //{
            //    this.WriteEnumerable(sb, val as IEnumerable);
            //}
            else if (val is Guid)
                sb.Append(((Guid)val).ToString("N"));
            else
            {
                throw new Exception("Unsupported");
            }
        }

        /// <summary>
        /// Writes a string as a JSON string
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="s"></param>
        static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append('"');
        }
    }
}
