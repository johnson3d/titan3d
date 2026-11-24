using System;
using System.IO;
using System.Security.Cryptography;

namespace Titan.Build
{
    /// <summary>
    /// 与仓库既有指纹口径一致：CRLF/LF 检出结果必须等价，因此先剔除 CR 再取 SHA256。
    /// </summary>
    public static class Sha
    {
        static readonly char[] Hex = "0123456789abcdef".ToCharArray();

        public static string OfFileWithoutCr(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var length = 0;
            for (var i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != (byte)'\r')
                    bytes[length++] = bytes[i];
            }
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(bytes, 0, length);
                return ToHex(hash);
            }
        }

        public static string OfString(string text)
        {
            using (var sha = SHA256.Create())
            {
                return ToHex(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text)));
            }
        }

        static string ToHex(byte[] hash)
        {
            var chars = new char[hash.Length * 2];
            for (var i = 0; i < hash.Length; i++)
            {
                chars[i * 2] = Hex[hash[i] >> 4];
                chars[i * 2 + 1] = Hex[hash[i] & 0x0F];
            }
            return new string(chars);
        }
    }
}
