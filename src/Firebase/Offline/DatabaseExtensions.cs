namespace Firebase.Database.Offline
{
    public static class DatabaseExtensions
    {
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
