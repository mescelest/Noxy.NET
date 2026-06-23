using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace Noxy.NET.Extensions;

public static class StringExtensions
{
    private static readonly byte[] Namespace = Guid.Parse("00000000-AAAA-BBBB-CCCC-DDDDEEEEFFFF").ToByteArray();
    private static readonly SearchValues<char> IllegalCharList = SearchValues.Create([.. Path.GetInvalidFileNameChars(), ' ']);

    extension(string value)
    {
        public string DefaultIfEmpty(string defaultValue = "")
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public string DefaultIfWhiteSpace(string defaultValue = "")
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        public Guid ToDeterministicGuid()
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(value);

            byte[] data = [.. Namespace, .. tokenBytes];
            byte[] hash = SHA1.HashData(data);

            hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

            return new([.. hash.Take(16)]);
        }

        public string ToEscapedSqlLike()
        {
            return value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
        }

        public string SanitizeFileName()
        {
            if (string.IsNullOrEmpty(value)) return value;

            ReadOnlySpan<char> source = value.AsSpan();
            int index = source.IndexOfAny(IllegalCharList);
            if (index == -1) return value;

            int validLength = index;
            for (int i = index; i < source.Length; i++)
            {
                if (!IllegalCharList.Contains(source[i]))
                {
                    validLength++;
                }
            }

            return string.Create(validLength, (value, index), (dest, state) =>
            {
                ReadOnlySpan<char> src = state.value.AsSpan();

                src[..state.index].CopyTo(dest);

                int destIdx = state.index;
                for (int i = state.index; i < src.Length; i++)
                {
                    char c = src[i];
                    if (!IllegalCharList.Contains(c))
                    {
                        dest[destIdx++] = c;
                    }
                }
            });
        }
    }
}
