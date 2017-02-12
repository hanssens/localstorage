using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hanssens.Net.Helpers;
using Newtonsoft.Json;

namespace Hanssens.Net
{
    /// <summary>
    /// A simple and lightweight tool for persisting data in dotnet (core) apps.
    /// </summary>
    public class LocalStorage : IDisposable
    {
        /// <summary>
        /// Gets the number of elements contained in the LocalStorage.
        /// </summary>
        public int Count => Storage.Count;

        /// <summary>
        /// Configurable behaviour for this LocalStorage instance.
        /// </summary>
        private readonly LocalStorageConfiguration _config;

        /// <summary>
        /// User-provided encryption key, used for encrypting/decrypting values.
        /// </summary>
        private readonly string _encryptionKey;

        /// <summary>
        /// Most current actual, in-memory state representation of the LocalStorage.
        /// </summary>
        private Dictionary<string, string> Storage { get; set; } = new Dictionary<string, string>();

        public LocalStorage() : this(new LocalStorageConfiguration()) { }

        public LocalStorage(LocalStorageConfiguration configuration) : this(configuration, string.Empty) { }

        public LocalStorage(LocalStorageConfiguration configuration, string encryptionKey)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _config = configuration;

            if (_config.EnableEncryption) {
                if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentNullException(nameof(encryptionKey), "When EnableEncryption is enabled, an encryptionKey is required when initializing the LocalStorage.");
                _encryptionKey = encryptionKey;
            }

            if (_config.AutoLoad)
                Load();
        }

        /// <summary>
        /// Clears the in-memory contents of the LocalStorage, but leaves any persisted state on disk intact.
        /// </summary>
        /// <remarks>
        /// Use the Destroy method to delete the persisted file on disk.
        /// </remarks>
        public void Clear()
        {
            Storage.Clear();
        }

        /// <summary>
        /// Deletes the persisted file on disk, if it exists, but keeps the in-memory data intact.
        /// </summary>
        /// <remarks>
        /// Use the Clear method to clear only the in-memory contents.
        /// </remarks>
        public void Destroy()
        {
            var filepath = FileHelpers.GetLocalStoreFilePath(_config.Filename);
            if (File.Exists(filepath))
                File.Delete(FileHelpers.GetLocalStoreFilePath(_config.Filename));
        }

        /// <summary>
        /// Gets an object from the LocalStorage, without knowing its type.
        /// </summary>
        /// <param name="key">Unique key, as used when the object was stored.</param>
        public object Get(string key)
        {
            return Get<object>(key);
        }

        /// <summary>
        /// Gets a strong typed object from the LocalStorage.
        /// </summary>
        /// <param name="key">Unique key, as used when the object was stored.</param>
        public T Get<T>(string key)
        {
            var succeeded = Storage.TryGetValue(key, out string raw);
            if (!succeeded) throw new ArgumentNullException($"Could not find key '{key}' in the LocalStorage.");

            if (_config.EnableEncryption)
                raw = CryptographyHelpers.Decrypt(_encryptionKey, raw);

            return JsonConvert.DeserializeObject<T>(raw);
        }

        /// <summary>
        /// Loads the persisted state from disk into memory, overriding the current memory instance.
        /// </summary>
        /// <remarks>
        /// Simply doesn't do anything if the file is not found on disk.
        /// </remarks>
        public void Load()
        {
            if (!File.Exists(FileHelpers.GetLocalStoreFilePath(_config.Filename))) return;

            var serializedContent = File.ReadAllText(FileHelpers.GetLocalStoreFilePath(_config.Filename));

            if (string.IsNullOrEmpty(serializedContent)) return;

            Storage.Clear();
            Storage = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedContent);
        }

        /// <summary>
        /// Stores an object into the LocalStorage.
        /// </summary>
        /// <param name="key">Unique key, can be any string, used for retrieving it later.</param>
        /// <param name="instance"></param>
        public void Store<T>(string key, T instance)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var value = JsonConvert.SerializeObject(instance);

            if (Storage.Keys.Contains(key))
                Storage.Remove(key);

            if (_config.EnableEncryption)
                value = CryptographyHelpers.Encrypt(_encryptionKey, value);

            Storage.Add(key, value);
        }

        /// <summary>
        /// Syntax sugar that transforms the response to an IEnumerable<T>, whilst also passing along an optional WHERE-clause. 
        /// </summary>
        public IEnumerable<T> Query<T>(string key, Func<T, bool> predicate = null)
        {
            var collection = Get<IEnumerable<T>>(key);
            return predicate == null ? collection : collection.Where(predicate);
        }

        /// <summary>
        /// Persists the in-memory store to disk.
        /// </summary>
        public void Persist()
        {
            var serialized = JsonConvert.SerializeObject(Storage);

            using (var fileStream = new FileStream(FileHelpers.GetLocalStoreFilePath(_config.Filename), FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(serialized);
                }
            }
        }

        public void Dispose()
        {
            if (_config.AutoSave)
                Persist();
        }
    }
}
