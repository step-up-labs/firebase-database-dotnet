namespace Firebase.Database.Offline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Firebase.Database.Query;

    public static class DatabaseExtensions
    {
        /// <summary>
        /// Create new instances of the <see cref="RealtimeDatabase{T}"/>.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <param name="filenameModifier"> Custom string which will get appended to the file name. </param>
        /// <param name="elementRoot"> Optional custom root element of received json items. </param>
        /// <param name="streamingOptions"> Realtime streaming options. </param> 
        /// <param name="initialPullStrategy"> Specifies what strategy should be used for initial pulling of server data. </param>
        /// <param name="pushChanges"> Specifies whether changed items should actually be pushed to the server. It this is false, then Put / Post / Delete will not affect server data. </param>
        /// <returns> The <see cref="RealtimeDatabase{T}"/>. </returns>
        public static RealtimeDatabase<T> AsRealtimeDatabase<T>(this ChildQuery query, string filenameModifier = "", string elementRoot = "", StreamingOptions streamingOptions = StreamingOptions.LatestOnly, InitialPullStrategy initialPullStrategy = InitialPullStrategy.MissingOnly, bool pushChanges = true)
            where T : class
        {
            return new RealtimeDatabase<T>(query, elementRoot, query.Client.Options.OfflineDatabaseFactory, filenameModifier, streamingOptions, initialPullStrategy, pushChanges);
        }

        /// <summary>
        /// Create new instances of the <see cref="RealtimeDatabase{T}"/>.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <typeparam name="TSetHandler"> Type of the custom <see cref="ISetHandler{T}"/> to use. </typeparam>
        /// <param name="filenameModifier"> Custom string which will get appended to the file name. </param>
        /// <param name="elementRoot"> Optional custom root element of received json items. </param>
        /// <param name="streamingOptions"> Realtime streaming options. </param> 
        /// <param name="initialPullStrategy"> Specifies what strategy should be used for initial pulling of server data. </param>
        /// <param name="pushChanges"> Specifies whether changed items should actually be pushed to the server. It this is false, then Put / Post / Delete will not affect server data. </param>
        /// <returns> The <see cref="RealtimeDatabase{T}"/>. </returns>
        public static RealtimeDatabase<T> AsRealtimeDatabase<T, TSetHandler>(this ChildQuery query, string filenameModifier = "", string elementRoot = "", StreamingOptions streamingOptions = StreamingOptions.LatestOnly, InitialPullStrategy initialPullStrategy = InitialPullStrategy.MissingOnly, bool pushChanges = true)
            where T : class
            where TSetHandler : ISetHandler<T>, new()
        {
            return new RealtimeDatabase<T>(query, elementRoot, query.Client.Options.OfflineDatabaseFactory, filenameModifier, streamingOptions, initialPullStrategy, pushChanges, Activator.CreateInstance<TSetHandler>());
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
            where T: class
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
            where T: class
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
            where T: class
        {
            db.Set(key, null, syncOnline ? SyncOptions.Put : SyncOptions.None, priority);
        }

        /// <summary>
        /// Do a Put for a nested property specified by <paramref name="propertyExpression"/> of an object with key <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T"> Type of the root elements. </typeparam>
        /// <typeparam name="TProperty"> Type of the property being modified</typeparam>
        /// <param name="db"> Database instance. </param>
        /// <param name="key"> Key of the root element to modify. </param>
        /// <param name="propertyExpression"> Expression on the root element leading to target value to modify. </param>
        /// <param name="value"> Value to put. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param> 
        public static void Put<T, TProperty>(this RealtimeDatabase<T> db, string key, Expression<Func<T, TProperty>> propertyExpression, TProperty value, bool syncOnline = true, int priority = 1)
            where T: class
        {
            db.Set(key, propertyExpression, value, syncOnline ? SyncOptions.Put : SyncOptions.None, priority);
        }

        /// <summary>
        /// Do a Patch for a nested property specified by <paramref name="propertyExpression"/> of an object with key <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T"> Type of the root elements. </typeparam>
        /// <typeparam name="TProperty"> Type of the property being modified</typeparam>
        /// <param name="db"> Database instance. </param>
        /// <param name="key"> Key of the root element to modify. </param>
        /// <param name="propertyExpression"> Expression on the root element leading to target value to modify. </param>
        /// <param name="value"> Value to patch. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param> 
        public static void Patch<T, TProperty>(this RealtimeDatabase<T> db, string key, Expression<Func<T, TProperty>> propertyExpression, TProperty value, bool syncOnline = true, int priority = 1)
            where T: class
        {
            db.Set(key, propertyExpression, value, syncOnline ? SyncOptions.Patch : SyncOptions.None, priority);
        }

        /// <summary>
        /// Delete a nested property specified by <paramref name="propertyExpression"/> of an object with key <paramref name="key"/>. This basically does a Put with null value.
        /// </summary>
        /// <typeparam name="T"> Type of the root elements. </typeparam>
        /// <typeparam name="TProperty"> Type of the property being modified</typeparam>
        /// <param name="db"> Database instance. </param>
        /// <param name="key"> Key of the root element to modify. </param>
        /// <param name="propertyExpression"> Expression on the root element leading to target value to modify. </param>
        /// <param name="value"> Value to put. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param> 
        public static void Delete<T, TProperty>(this RealtimeDatabase<T> db, string key, Expression<Func<T, TProperty>> propertyExpression, bool syncOnline = true, int priority = 1)
            where T: class
            where TProperty: class
        {
            db.Set(key, propertyExpression, null, syncOnline ? SyncOptions.Put : SyncOptions.None, priority);
        }

        /// <summary>
        /// Post a new entity into the nested dictionary specified by <paramref name="propertyExpression"/> of an object with key <paramref name="key"/>. 
        /// The key of the new entity is automatically generated.
        /// </summary>
        /// <typeparam name="T"> Type of the root elements. </typeparam>
        /// <typeparam name="TSelector"> Type of the dictionary being modified</typeparam>
        /// <typeparam name="TProperty"> Type of the value within the dictionary being modified</typeparam>
        /// <param name="db"> Database instance. </param>
        /// <param name="key"> Key of the root element to modify. </param>
        /// <param name="propertyExpression"> Expression on the root element leading to target value to modify. </param>
        /// <param name="value"> Value to put. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param> 
        public static void Post<T, TSelector, TProperty>(this RealtimeDatabase<T> db, string key, Expression<Func<T, TSelector>> propertyExpression, TProperty value, bool syncOnline = true, int priority = 1)
            where T: class
            where TSelector: IDictionary<string, TProperty>
        {
            var nextKey = FirebaseKeyGenerator.Next();
            var expression = Expression.Lambda<Func<T, TProperty>>(Expression.Call(propertyExpression.Body, typeof(TSelector).GetRuntimeMethod("get_Item", new[] { typeof(string) }), Expression.Constant(nextKey)), propertyExpression.Parameters);
            db.Set(key, expression, value, syncOnline ? SyncOptions.Put : SyncOptions.None, priority);
        }

        /// <summary>
        /// Delete an entity with key <paramref name="dictionaryKey"/> in the nested dictionary specified by <paramref name="propertyExpression"/> of an object with key <paramref name="key"/>. 
        /// The key of the new entity is automatically generated.
        /// </summary>
        /// <typeparam name="T"> Type of the root elements. </typeparam>
        /// <typeparam name="TSelector"> Type of the dictionary being modified</typeparam>
        /// <typeparam name="TProperty"> Type of the value within the dictionary being modified</typeparam>
        /// <param name="db"> Database instance. </param>
        /// <param name="key"> Key of the root element to modify. </param>
        /// <param name="propertyExpression"> Expression on the root element leading to target value to modify. </param>
        /// <param name="dictionaryKey"> Key within the nested dictionary to delete. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param> 
        public static void Delete<T, TProperty>(this RealtimeDatabase<T> db, string key, Expression<Func<T, IDictionary<string, TProperty>>> propertyExpression, string dictionaryKey, bool syncOnline = true, int priority = 1)
            where T: class
        {
            var expression = Expression.Lambda<Func<T, TProperty>>(Expression.Call(propertyExpression.Body, typeof(IDictionary<string, TProperty>).GetRuntimeMethod("get_Item", new[] { typeof(string) }), Expression.Constant(dictionaryKey)), propertyExpression.Parameters);
            db.Set(key, expression, null, syncOnline ? SyncOptions.Put : SyncOptions.None, priority);
        }
    }
}
