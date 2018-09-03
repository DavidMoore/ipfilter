using System;
using System.Security.Cryptography;
using System.Text;

namespace IPFilter.Cli
{
    static class CryptographicAlgorithmExtensions
    {
        public static string ComputeHash(this HashAlgorithm hashAlgorithm, object value)
        {
            return ComputeHash(hashAlgorithm, value.ToString());
        }

        public static string ComputeHash(this HashAlgorithm hashAlgorithm, string value)
        {
            return BitConverter.ToString(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }
}