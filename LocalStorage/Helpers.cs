using System;
using System.IO;

namespace Hanssens.Net
{
    internal class Helpers
    {
        internal static string GetLocalStoreFilePath(string filename)
        {
            return Path.Combine(System.AppContext.BaseDirectory, filename);
        }

        internal static string Decrypt(string encryptionKey, string encryptedValue)
        {
            throw new NotImplementedException();
        }

        internal static string Encrypt(string encryptionKey, string rawValue)
        {
            throw new NotImplementedException();
        }
    }
}
