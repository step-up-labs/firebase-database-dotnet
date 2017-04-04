namespace Firebase.Database.Offline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class OfflineCacheAdapter<TKey, T> : IDictionary<string, T>, IDictionary 
    {
        private readonly IDictionary<string, OfflineEntry> database;

        public OfflineCacheAdapter(IDictionary<string, OfflineEntry> database)
        {
            this.database = database;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count => this.database.Count;

        public bool IsSynchronized { get; }

        public object SyncRoot { get; }

        public bool IsReadOnly => this.database.IsReadOnly;

        object IDictionary.this[object key]
        {
            get
            {
                return this.database[key.ToString()].Deserialize<T>();
            }

            set
            {
                var keyString = key.ToString();
                if (this.database.ContainsKey(keyString))
                {
                    this.database[keyString] = new OfflineEntry(keyString, value, this.database[keyString].Priority, this.database[keyString].SyncOptions);
                }
                else
                {
                    this.database[keyString] = new OfflineEntry(keyString, value, 1, SyncOptions.None);
                }
            }
        }

        public ICollection<string> Keys => this.database.Keys;

        ICollection IDictionary.Values { get; }

        ICollection IDictionary.Keys { get; }

        public ICollection<T> Values => this.database.Values.Select(o => o.Deserialize<T>()).ToList();

        public T this[string key]
        {
            get
            {
                return this.database[key].Deserialize<T>();
            }

            set
            {
                if (this.database.ContainsKey(key))
                {
                    this.database[key] = new OfflineEntry(key, value, this.database[key].Priority, this.database[key].SyncOptions);
                }
                else
                {
                    this.database[key] = new OfflineEntry(key, value, 1, SyncOptions.None);
                }
            }
        }

        public bool Contains(object key)
        {
            return this.ContainsKey(key.ToString());
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            this.Remove(key.ToString());
        }

        public bool IsFixedSize => false;

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return this.database.Select(d => new KeyValuePair<string, T>(d.Key, d.Value.Deserialize<T>())).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(KeyValuePair<string, T> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(object key, object value)
        {
            this.Add(key.ToString(), (T)value);
        }

        public void Clear()
        {
            this.database.Clear();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return this.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return this.database.Remove(item.Key);
        }

        public void Add(string key, T value)
        {
            this.database.Add(key, new OfflineEntry(key, value, 1, SyncOptions.None));
        }

        public bool ContainsKey(string key)
        {
            return this.database.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.database.Remove(key);
        }

        public bool TryGetValue(string key, out T value)
        {
            OfflineEntry val;

            if (this.database.TryGetValue(key, out val))
            {
                value = val.Deserialize<T>();
                return true;
            }

            value = default(T);
            return false;
        }
    }
}
