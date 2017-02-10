using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace LocalStorage
{    
    public class LocalStorage
    {
        /// <summary>
        /// Most current actual, in-memory state representation of the LocalStorage.
        /// </summary>
        private Dictionary<string, string> Storage { get; set; } = new Dictionary<string, string>();

        public LocalStorage()
        {
            if (File.Exists(GetLocalStoreFilePath()))
            {
                var serializedContent = File.ReadAllText(GetLocalStoreFilePath());

                if (string.IsNullOrEmpty(serializedContent)) return;

                Storage = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedContent);
            }
        }

        /// <summary>
        /// Clears the contents of the LocalStorage, both in-memory as well as the persisted state on disk.
        /// </summary>
        public void Clear()
        {
            Storage.Clear();
            File.WriteAllText(GetLocalStoreFilePath(), string.Empty);
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
            if (succeeded) return JsonConvert.DeserializeObject<T>(raw);

            throw new ArgumentNullException($"Could not find key '{key}' in the LocalStorage.");
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

        internal string GetLocalStoreFilePath()
        {
            const string filename = ".localstorage";
            var fullPath = Path.Combine(System.AppContext.BaseDirectory, filename);
            return fullPath;
        }

        /// <summary>
        /// Persists the in-memory store to disk.
        /// </summary>
        public void Persist()
        {
            var serialized = JsonConvert.SerializeObject(Storage);

            using (var fileStream = new FileStream(GetLocalStoreFilePath(), FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(serialized);
                }
            }
        }
    }
}
