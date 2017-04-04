namespace Firebase.Database.Offline
{
    using Firebase.Database.Query;

    public static class DatabaseExtensions
    {
        /// <summary>
        /// Create new instances of the <see cref="RealtimeDatabase{T}"/>.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <param name="filenameModifier"> Custom string which will get appended to the file name. </param>
        /// <param name="elementRoot"> Optional custom root element of received json items. </param>
        /// <param name="streamChanges"> Specifies whether changes should be streamed from the server. </param> 
        /// <param name="initialPullStrategy"> Specifies what strategy should be used for initial pulling of server data. </param>
        /// <param name="pushChanges"> Specifies whether changed items should actually be pushed to the server. It this is false, then Put / Post / Delete will not affect server data. </param>
        /// <returns> The <see cref="RealtimeDatabase{T}"/>. </returns>
        public static RealtimeDatabase<T> AsRealtimeDatabase<T>(this ChildQuery query, string filenameModifier = "", string elementRoot = "", bool streamChanges = true, InitialPullStrategy initialPullStrategy = InitialPullStrategy.MissingOnly, bool pushChanges = true) 
            where T: class
        {
            return new RealtimeDatabase<T>(query, elementRoot, query.Client.Options.OfflineDatabaseFactory, filenameModifier, streamChanges, initialPullStrategy, pushChanges);
        }

        /// <summary>
        /// Overwrites existing object with given key leaving any missing properties intact in firebase.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="obj"> The object to set. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param>
        public static void Patch<T>(this RealtimeDatabase<T> db, string key, T obj, bool syncOnline = true, int priority = 1)
            where T: class
        {
            db.Set(key, obj, syncOnline ? SyncOptions.Patch : SyncOptions.None, priority);
        }

        /// <summary>
        /// Overwrites existing object with given key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="obj"> The object to set. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param>
        public static void Put<T>(this RealtimeDatabase<T> db, string key, T obj, bool syncOnline = true, int priority = 1)
            where T : class
        {
            db.Set(key, obj, syncOnline ? SyncOptions.Put : SyncOptions.None, priority);
        }

        /// <summary>
        /// Adds a new entity to the Database.
        /// </summary>
        /// <param name="obj"> The object to add.  </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param>
        /// <returns> The generated key for this object. </returns>
        public static string Post<T>(this RealtimeDatabase<T> db, T obj, bool syncOnline = true, int priority = 1)
            where T : class
        {
            var key = FirebaseKeyGenerator.Next();

            db.Set(key, obj, syncOnline ? SyncOptions.Put : SyncOptions.None, priority);

            return key;
        }

        /// <summary>
        /// Deletes the entity with the given key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param> 
        public static void Delete<T>(this RealtimeDatabase<T> db, string key, bool syncOnline = true, int priority = 1)
            where T : class
        {
            db.Set(key, null, syncOnline ? SyncOptions.Put : SyncOptions.None, priority);
        }
    }
}
