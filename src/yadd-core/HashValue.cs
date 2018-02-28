using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace yadd.core
{
    public enum HashKind : byte
    {
        Undefined = 0xFF,
        Null = 0,
        SHA1 = 0xA1
    }

    public class HashValue
    {
        private byte[] hashBytes;

        public static HashValue Null = new HashValue();

        private HashValue() { Kind = HashKind.Null; }

        public HashValue(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var sha = new SHA1Managed())
            {
                hashBytes = sha.ComputeHash(bytes);
            }
            Kind = HashKind.SHA1;
        }

        public HashKind Kind { get; private set; }

        public override string ToString()
        {
            if (Kind == HashKind.Null)
                return string.Empty;
            string tag = Kind.ToString("x");
            return hashBytes.ToHexString() + tag;
        }
    }
}
