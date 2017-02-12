using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hanssens.Net.Helpers
{
    /// <summary>
    /// Helpers for encrypting and decrypting.
    /// </summary>
    /// <remarks>
    /// Originally inspired by https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged(v=vs.110).aspx
    /// (although changed substantially due to *not* using Rijndael)
    /// </remarks>
    internal class CryptographyHelpers
    {
        internal static string Decrypt(string password, string salt, string encrypted_value)
        {
            string decrypted;
            
            using (var aes = Aes.Create())
            {
                var keys = GetAesKeyAndIV(password, salt, aes);
                aes.Key = keys.Item1;
                aes.IV = keys.Item2;

                // Create a decrytor to perform the stream transform.
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                var encrypted_bytes = ToByteArray(encrypted_value);
                using (var memory_stream = new MemoryStream(encrypted_bytes))
                {
                    using (var crypto_stream = new CryptoStream(memory_stream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var reader = new StreamReader(crypto_stream))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            decrypted = reader.ReadToEnd();
                        }
                    }
                }
            }

            return decrypted;
        }

        internal static string Encrypt(string password, string salt, string plain_text)
        {
            string encrypted;

            using (var aes = Aes.Create())
            {
                var keys = GetAesKeyAndIV(password, salt, aes);
                aes.Key = keys.Item1;
                aes.IV = keys.Item2;

                // Create a decrytor to perform the stream transform.
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using (var memory_stream = new MemoryStream())
                {
                    using (var crypto_stream = new CryptoStream(memory_stream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var writer = new StreamWriter(crypto_stream))
                        {
                            // write all data to the stream.
                            writer.Write(plain_text);
                        }

                        var encrypted_bytes = memory_stream.ToArray();
                        encrypted = ToString(encrypted_bytes);
                    }
                }
            }

            return encrypted;
        }

        private static byte[] ToByteArray(string input)
        {
            return Encoding.Unicode.GetBytes(input);
        }

        private static string ToString(byte[] input)
        {
            return Encoding.Unicode.GetString(input);
        }

        private static Tuple<byte[], byte[]> GetAesKeyAndIV(string password, string salt, SymmetricAlgorithm symmetricAlgorithm)
        {
            // inspired by @troyhunt: https://www.troyhunt.com/owasp-top-10-for-net-developers-part-7/
            const int bits = 8;
            var key = new byte[16];
            var iv = new byte[16];

            var derive_bytes = new Rfc2898DeriveBytes(password, ToByteArray(salt));
            key = derive_bytes.GetBytes(symmetricAlgorithm.KeySize / bits);
            iv = derive_bytes.GetBytes(symmetricAlgorithm.BlockSize / bits);

            return new Tuple<byte[], byte[]>(key, iv);
        }

    }
}