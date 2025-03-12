using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace SharedKernel.Extensions.Encryption;

public class RsaEncryptionExtension
{
    public static string Encrypt(
        string text,
        string publicKey,
        RSAEncryptionPadding? padding = null,
        int keySize = 2048
    )
    {
        padding ??= RSAEncryptionPadding.OaepSHA1;
        byte[] encypted = null!;
        using RSACryptoServiceProvider rsaEncrypt = new(keySize);
        try
        {
            RSAParameters rsaParameters = GetPublicKey(publicKey);

            rsaEncrypt.ImportParameters(rsaParameters);
            encypted = rsaEncrypt.Encrypt(Encoding.UTF8.GetBytes(text), padding);
        }
        finally
        {
            rsaEncrypt.PersistKeyInCsp = false;
        }

        return Convert.ToBase64String(encypted);
    }

    public static string Decrypt(string encrypted, string privateKey)
    {
        byte[] decryptedBytes = null!;
        using RSACryptoServiceProvider rsaDecrypte = new(2048);
        try
        {
            RSAParameters rsaParameters = GetPrivateKey(privateKey);

            rsaDecrypte.ImportParameters(rsaParameters);

            decryptedBytes = rsaDecrypte.Decrypt(Convert.FromBase64String(encrypted), true);
        }
        finally
        {
            rsaDecrypte.PersistKeyInCsp = false;
        }

        return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
    }

    private static RSAParameters GetPublicKey(string publicKey)
    {
        using TextReader publicKeyStringReader = new StringReader(publicKey);
        RsaKeyParameters pemReader = (RsaKeyParameters)
            new PemReader(publicKeyStringReader).ReadObject();

        return DotNetUtilities.ToRSAParameters(pemReader);
    }

    private static RSAParameters GetPrivateKey(string privateKey)
    {
        using TextReader privateKeyStringReader = new StringReader(privateKey);
        AsymmetricCipherKeyPair pemReader = (AsymmetricCipherKeyPair)
            new PemReader(privateKeyStringReader).ReadObject();

        return DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)pemReader.Private);
    }
}
