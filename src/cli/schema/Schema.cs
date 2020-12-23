using CommandDotNet;
using CommandDotNet.Rendering;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using yadd.core;
using yadd.postgresql_provider;

namespace yadd.cli.schema
{
    [Command(Description = "Manages Database Schema.")]
    public class Schema
    {
        [Command(Description = "PRIVATE")]
        // https://jeremybytes.blogspot.com/2020/01/dynamically-loading-types-in-net-core.html
        public void TEST(IConsole console, CancellationToken cancellationToken)
        {
            IProvider provider = new PostgreSQLProvider();
            var schema = provider.DataDefinition.GetInformationSchema();
            string jsonString = JsonSerializer.Serialize(schema);
            console.WriteLine(jsonString);
            using var sha256Hash = SHA256.Create();
            string hash = GetHash(sha256Hash, jsonString);
            console.WriteLine(hash);
        }

        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
