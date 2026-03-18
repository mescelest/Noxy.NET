using System.Security.Cryptography;
using System.Text;

namespace Noxy.NET.Extensions;

public static class StringExtensions
{
    public static string DefaultIfEmpty(this string value, string defaultValue = "")
    {
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    public static string DefaultIfWhiteSpace(this string value, string defaultValue = "")
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static readonly byte[] Namespace = Guid.Parse("00000000-AAAA-BBBB-CCCC-DDDDEEEEFFFF").ToByteArray();

    public static Guid ToDeterministicGuid(this string token)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

        byte[] data = Namespace.Concat(tokenBytes).ToArray();
        byte[] hash = SHA1.HashData(data);

        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new(hash.Take(16).ToArray());
    }
}
