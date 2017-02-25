using System;
using System.IO;

namespace Hanssens.Net.Helpers
{
    internal static class FileHelpers
    {
        internal static string GetLocalStoreFilePath(string filename)
        {
            return Path.Combine(System.AppContext.BaseDirectory, filename);
        }
    }
}
