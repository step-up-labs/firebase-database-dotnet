namespace Firebase.Database.Offline
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    using Firebase.Database.Query;
    using Firebase.Database.Streaming;

    /// <summary>
    /// The real-time database which synchronizes online and offline data. 
    /// </summary>
    /// <typeparam name="T"> Type of entities. </typeparam>
    public partial class RealtimeDatabase<T> where T : class
    {
        private readonly ChildQuery childQuery;
        private readonly bool streamChanges;
        private readonly IDictionary<string, OfflineEntry> database;
        private readonly Subject<FirebaseEvent<T>> subject;

        private IDisposable subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealtimeDatabase{T}"/> class.
        /// </summary>
        /// <param name="childQuery"> The child query. </param>
        /// <param name="offlineDatabaseFactory"> The offline database factory. </param>
        /// <param name="filenameModifier"> Custom string which will get appended to the file name. </param> 
        /// <param name="streamChanges"> Specifies whether changes should be streamed from the server. </param> 
        public RealtimeDatabase(ChildQuery childQuery, Func<Type, string, IDictionary<string, OfflineEntry>> offlineDatabaseFactory, string filenameModifier, bool streamChanges)
        {
            this.childQuery = childQuery;
            this.streamChanges = streamChanges;
            this.subject = new Subject<FirebaseEvent<T>>();
            this.database = offlineDatabaseFactory(typeof(T), filenameModifier);
        }

        /// <summary>
        /// Overwrites existing object with given key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="obj"> The object to set. </param>
        public void Put(string key, T obj)
        {
            if (this.database.ContainsKey(key))
            {
                this.database[key].SyncOptions = SyncOptions.Push;
                this.SetAndRaise(key, this.database[key]);
            }
            else
            {
                this.SetAndRaise(key, new OfflineEntry(key, obj));
            }
        }

        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="obj"> The object to add.  </param>
        /// <returns> The generated key for this object. </returns>
        public string Post(T obj)
        {
            var key = FirebaseKeyGenerator.Next();

            this.SetAndRaise(key, new OfflineEntry(key, obj));

            return key;
        }

        /// <summary>
        /// Deletes the entity with the given key.
        /// </summary>
        /// <param name="key"> The key. </param>
        public void Delete(string key)
        {
            this.SetAndRaise(key, new OfflineEntry(key, null));
        }

        /// <summary>
        /// Fetches an object with the given key and adds it to the database.
        /// </summary>
        /// <param name="key"> The key. </param>
        public void Pull(string key)
        {
            if (!this.database.ContainsKey(key))
            {
                this.database[key] = new OfflineEntry(key, null, SyncOptions.Pull);
            }
            else
            {
                this.database[key].SyncOptions = SyncOptions.Pull;
            }
        }

        /// <summary> 
        /// Starts observing the real-time database. Events will be fired both when change is done locally and remotely.
        /// </summary> 
        /// <returns> Stream of <see cref="FirebaseEvent{T}"/>. </returns>
        public IObservable<FirebaseEvent<T>> AsObservable()
        {
            Task.Factory.StartNew(this.SynchronizeThread, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            var initialData = this.database
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value.Data) && kvp.Value.Data != "null")
                .Select(kvp => new FirebaseEvent<T>(kvp.Key, kvp.Value.Deserialize<T>(), FirebaseEventType.InsertOrUpdate))
                .ToList()
                .ToObservable();

            return initialData.Concat(this.subject); 
        }

        private void SetAndRaise(string key, OfflineEntry obj)
        {
            this.database[key] = obj;
            this.subject.OnNext(new FirebaseEvent<T>(key, obj?.Deserialize<T>(), string.IsNullOrEmpty(obj?.Data) ? FirebaseEventType.Delete : FirebaseEventType.InsertOrUpdate));
        }

        private async void SynchronizeThread()
        {
            if (this.subscription != null)
            {
                return;
            }

            if (this.streamChanges)
            { 
                this.subscription = new FirebaseSubscription<T>(this.subject, this.childQuery.OrderByKey().StartAt(() => this.GetLatestKey()), new FirebaseCache<T>(new OfflineCacheAdapter<string, T>(this.database))).Run();
            }
            else
            {
                this.subscription = Observable.Empty<string>().Subscribe(); // just a dummy IDisposable
            }

            while (true)
            {
                try
                {
                    var validEntries = this.database.Where(e => e.Value != null);
                    await this.PullEntriesAsync(validEntries.Where(kvp => kvp.Value.SyncOptions == SyncOptions.Pull).Select(kvp => kvp.Key));
                    await this.PushEntriesAsync(validEntries.Where(kvp => kvp.Value.SyncOptions == SyncOptions.Push));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                await Task.Delay(1000);
            }
        }

        private string GetLatestKey()
        {
            return this.database.OrderBy(o => o.Key, StringComparer.Ordinal).LastOrDefault().Key;
        }

        private async Task PushEntriesAsync(IEnumerable<KeyValuePair<string, OfflineEntry>> pushEntries)
        {
            var tasks = pushEntries.Select(kvp => this.childQuery.Child(kvp.Key).PutAsync(kvp.Value.Deserialize<T>())).ToList();

            await Task.WhenAll(tasks);

            this.ResetSyncOptions(pushEntries.Select(s => s.Key));
        }

        private async Task PullEntriesAsync(IEnumerable<string> pullEntries)
        {
            var tasks = pullEntries.Select(key => new { Key = key, Task = this.childQuery.Child(key).OnceSingleAsync<T>() }).ToList();

            await Task.WhenAll(tasks.Select(t => t.Task));

            foreach (var task in tasks)
            {
                this.SetAndRaise(task.Key, new OfflineEntry(task.Key, task.Task.Result, SyncOptions.None));
            }
        }

        private void ResetSyncOptions(IEnumerable<string> entries)
        {
            foreach (var key in entries)
            {
                var item = this.database[key];
                item.SyncOptions = SyncOptions.None;
                this.database[key] = item;
            }
        }
    }
}
