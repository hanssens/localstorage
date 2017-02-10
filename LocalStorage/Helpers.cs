using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LocalStorage
{
    internal class Helpers
    {
        internal static string GetLocalStoreFilePath(string filename)
        {
            return Path.Combine(System.AppContext.BaseDirectory, filename);
        }
    }
}
