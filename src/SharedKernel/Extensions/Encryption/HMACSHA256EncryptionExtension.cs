using System.Security.Cryptography;
using System.Text;

namespace SharedKernel.Extensions.Encryption;

public static class HMACSHA256EncryptionExtension
{
    public static string ComputeHMACSHA256(string input, string key)
    {
        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(key));
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = hmac.ComputeHash(inputBytes);

        // Convert byte array to hexadecimal string
        StringBuilder sb = new();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("x2")); // "x2" ensures 2-digit hex format
        }
        return sb.ToString();
    }

    public static bool VerifyHMACSHA256(string input, string key, string providedHash)
    {
        string computedHash = ComputeHMACSHA256(input, key);
        return string.Equals(computedHash, providedHash, StringComparison.OrdinalIgnoreCase);
    }
}
