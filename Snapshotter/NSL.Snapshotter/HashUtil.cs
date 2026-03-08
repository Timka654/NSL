using System.Security.Cryptography;
using System.Text;

namespace NSL.Snapshotter
{
    // =============================
    // 5) Utilities
    // =============================
    public static class HashUtil
    {
        public static string ComputeSha256Hex(ReadOnlySpan<byte> bytes)
        {
            Span<byte> hash = stackalloc byte[32];
            SHA256.HashData(bytes, hash);

            // lowercase hex
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static string FormatHashLine(string algoLower, string hexLower) => $"{algoLower}:{hexLower}";

        public static (string algo, string hex) ParseHashLine(string text)
        {
            var line = (text ?? string.Empty).Trim(); // tolerant to accidental whitespace/newline
            var idx = line.IndexOf(':');
            if (idx <= 0 || idx == line.Length - 1)
                throw new InvalidOperationException($"Invalid hash format. Expected 'algo:hex'. Got: '{line}'");

            var algo = line[..idx].Trim().ToLowerInvariant();
            var hex = line[(idx + 1)..].Trim().ToLowerInvariant();

            if (algo != "sha256")
                throw new InvalidOperationException($"Unsupported hash algorithm '{algo}'. Supported: sha256.");

            // minimal hex validation
            if (hex.Length == 0 || (hex.Length % 2) != 0 || hex.Any(c => !"0123456789abcdef".Contains(c)))
                throw new InvalidOperationException("Invalid hash hex.");

            return (algo, hex);
        }
    }

}
