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
    public class LocalStorage : IDisposable, ILocalStorage
    {
        public int Count => Storage.Count;
        private readonly ILocalStorageConfiguration _config;
        private readonly string _encryptionKey;
        private Store Storage { get; set; } = new Store();
        
        private object writeLock = new object();

        /// <summary>
        /// Initializes a new instance of LocalStorage, with default conventions.
        /// </summary>
        public LocalStorage() : this(new LocalStorageConfiguration(), string.Empty) { }

        /// <summary>
        /// Initializes a new instance of LocalStorage, with specific configuration options.
        /// </summary>
        /// <param name="configuration">Custom configuration options</param>
        public LocalStorage(ILocalStorageConfiguration configuration) : this(configuration, string.Empty) { }

        /// <summary>
        /// Initializes a new *encrypted* instance of LocalStorage, with specific configuration options.
        /// </summary>
        /// <param name="configuration">Custom configuration options</param>
        /// <param name="encryptionKey">Custom encryption key</param>
        public LocalStorage(ILocalStorageConfiguration configuration, string encryptionKey)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (_config.EnableEncryption) {
                if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentNullException(nameof(encryptionKey), "When EnableEncryption is enabled, an encryptionKey is required when initializing the LocalStorage.");
                _encryptionKey = encryptionKey;
            }

            if (_config.AutoLoad)
                Load();
        }

        public void Clear()
        {
            Storage.Clear();
        }

        public void Destroy()
        {
            var filepath = FileHelpers.GetLocalStoreFilePath(_config.Filename);
            if (File.Exists(filepath))
                File.Delete(FileHelpers.GetLocalStoreFilePath(_config.Filename));
        }

        public bool Exists(string key)
        {
            return Storage.ContainsKey(key: key);
        }

        public object Get(string key)
        {
            return Get<object>(key);
        }

        public T Get<T>(string key)
        {
            var succeeded = Storage.TryGetValue(key, out string raw);
            if (!succeeded) throw new ArgumentNullException($"Could not find key '{key}' in the LocalStorage.");

            if (_config.EnableEncryption)
                raw = CryptographyHelpers.Decrypt(_encryptionKey, _config.EncryptionSalt, raw);

            return JsonConvert.DeserializeObject<T>(raw);
        }

        public IReadOnlyCollection<string> Keys()
        {
            return Storage.Keys.OrderBy(x => x).ToList();
        }

        public void Load()
        {
            if (!File.Exists(FileHelpers.GetLocalStoreFilePath(_config.Filename))) return;

            var serializedContent = File.ReadAllText(FileHelpers.GetLocalStoreFilePath(_config.Filename));

            if (string.IsNullOrEmpty(serializedContent)) return;

            Storage.Clear();
            Storage = JsonConvert.DeserializeObject<Store>(serializedContent);
        }

        public void Store<T>(string key, T instance)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var value = JsonConvert.SerializeObject(instance);

            if (Storage.Keys.Contains(key))
                Storage.Remove(key);

            if (_config.EnableEncryption)
                value = CryptographyHelpers.Encrypt(_encryptionKey, _config.EncryptionSalt, value);

            Storage.Add(key, value);
        }

        public IEnumerable<T> Query<T>(string key, Func<T, bool> predicate = null)
        {
            var collection = Get<IEnumerable<T>>(key);
            return predicate == null ? collection : collection.Where(predicate);
        }

        public void Persist()
        {
            var serialized = JsonConvert.SerializeObject(Storage, Formatting.Indented);

            var writemode = File.Exists(FileHelpers.GetLocalStoreFilePath(_config.Filename))
                ? FileMode.Truncate
                : FileMode.Create;
            
            lock (writeLock)
            {
                using (var fileStream = new FileStream(FileHelpers.GetLocalStoreFilePath(_config.Filename), 
                    mode: writemode, 
                    access: FileAccess.Write))
                {
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.Write(serialized);
                    }
                }
            }
        }

        public void Remove(string key)
        {
            Storage.Remove(key);
        }

        public void Dispose()
        {
            if (_config.AutoSave)
                Persist();
        }
    }
}
