namespace Firebase.Database.Offline
{
    using System;

    using Newtonsoft.Json;

    /// <summary>
    /// Represents an object stored in offline storage.
    /// </summary>
    public class OfflineEntry
    {
        private object dataInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="OfflineEntry"/> class with an already serialized object.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="obj"> The object. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param>  
        /// <param name="syncOptions"> The sync options. </param>
        public OfflineEntry(string key, object obj, string data, int priority, SyncOptions syncOptions, bool isPartial = false)
        {
            this.Key = key;
            this.Priority = priority;
            this.Data = data;
            this.Timestamp = DateTime.UtcNow;
            this.SyncOptions = syncOptions;
            this.IsPartial = isPartial;

            this.dataInstance = obj;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OfflineEntry"/> class.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="obj"> The object. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param>  
        /// <param name="syncOptions"> The sync options. </param>
        public OfflineEntry(string key, object obj, int priority, SyncOptions syncOptions, bool isPartial = false)
            : this(key, obj, JsonConvert.SerializeObject(obj), priority, syncOptions, isPartial)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OfflineEntry"/> class.
        /// </summary>
        public OfflineEntry() 
        {
        }

        /// <summary>
        /// Gets or sets the key of this entry.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the priority. Objects with higher priority will be synced first. Higher number indicates higher priority. 
        /// </summary>
        public int Priority 
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timestamp when this entry was last touched.
        /// </summary>
        public DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="SyncOptions"/> which define what sync state this entry is in.
        /// </summary>
        public SyncOptions SyncOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets serialized JSON data. 
        /// </summary>
        public string Data
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether this is only a partial object.
        /// </summary>
        public bool IsPartial
        {
            get;
            set;
        }

        /// <summary>
        /// Deserializes <see cref="Data"/> into <typeparamref name="T"/>. The result is cached.
        /// </summary>
        /// <typeparam name="T"> Type of object to deserialize into. </typeparam>
        /// <returns> Instance of <typeparamref name="T"/>. </returns>
        public T Deserialize<T>()
        {
            return (T)(this.dataInstance ?? (this.dataInstance = JsonConvert.DeserializeObject<T>(this.Data)));
        }
    }
}
