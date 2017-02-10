using System;
using System.Collections.Generic;
using System.Text;

namespace LocalStorage
{
    /// <summary>
    /// Provides options to configure LocalStorage to behave just like you want it.
    /// </summary>
    public class LocalStorageConfiguration : ILocalStorageConfiguration
    {
        /// <summary>
        /// Filename for the persisted state on disk (defaults to ".localstorage").
        /// </summary>
        public string Filename { get; set; } = ".localstorage";
    }
}
