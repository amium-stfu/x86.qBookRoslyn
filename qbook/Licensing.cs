using System;
using System.Security.Cryptography;
using System.Text;

namespace qbook
{
    internal class Licensing
    {
        public static string Encrypt(string PlainText, String Salt = "am#07", String InitialVector = "4vJ6Q9VsxPUaPgub")
        {
            if (string.IsNullOrEmpty(PlainText))
            {
                return string.Empty;
            }
            SHA256Managed sHA256Managed = new SHA256Managed();
            byte[] key = sHA256Managed.ComputeHash(Encoding.ASCII.GetBytes(Salt));
            byte[] iv = Encoding.ASCII.GetBytes(InitialVector);

            using (var SymmetricKey = new AesCng()
            {
                Key = key,
                IV = iv,
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
            })
            {
                using (var transform = SymmetricKey.CreateEncryptor(key, iv))
                {
                    var inputBytes = Encoding.UTF8.GetBytes(PlainText);
                    var encryptedBytes = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public static string Decrypt(string CipherText, String Salt = "am#07", String InitialVector = "4vJ6Q9VsxPUaPgub")
        {
            if (string.IsNullOrEmpty(CipherText))
            {
                return string.Empty;
            }
            SHA256Managed sHA256Managed = new SHA256Managed();
            byte[] key = sHA256Managed.ComputeHash(Encoding.ASCII.GetBytes(Salt));
            byte[] iv = Encoding.ASCII.GetBytes(InitialVector);

            using (var SymmetricKey = new AesCng()
            {
                Key = key,
                IV = iv,
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
            })
            {
                using (var transform = SymmetricKey.CreateDecryptor(key, iv))
                {
                    var inputBytes = Convert.FromBase64String(CipherText);
                    var PlainTextBytes = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Encoding.UTF8.GetString(PlainTextBytes);
                }
            }
        }

    }
}
