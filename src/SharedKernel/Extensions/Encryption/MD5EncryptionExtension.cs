using System.Security.Cryptography;
using System.Text;

namespace SharedKernel.Extensions.Encryption;

public class MD5EncryptionExtension
{
    public static string Encrypt(string input)
    {
        byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
