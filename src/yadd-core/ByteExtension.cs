using System;
using System.Collections.Generic;
using System.Text;

namespace yadd.core
{
    public static class ByteExtension
    {
        public static string ToHexString(this byte[] buf)
        {
            return BitConverter.ToString(buf).Replace("-", string.Empty);
        }
    }
}
