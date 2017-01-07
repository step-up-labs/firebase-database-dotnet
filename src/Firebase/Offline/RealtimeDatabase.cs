namespace Firebase.Database.Offline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    using Firebase.Database.Extensions;
    using Firebase.Database.Query;
    using Firebase.Database.Streaming;
    using System.Reactive.Threading.Tasks;

    /// <summary>
    /// The real-time Database which synchronizes online and offline data. 
    /// </summary>
    /// <typeparam name="T"> Type of entities. </typeparam>
    public partial class RealtimeDatabase<T> where T : class
    {
        private readonly ChildQuery childQuery;
        private readonly bool streamChanges;
        private readonly string elementRoot;
        private readonly Subject<FirebaseEvent<T>> subject;
        private readonly InitialPullStrategy initialPullStrategy;
        private readonly bool pushChanges;

        private IObservable<FirebaseEvent<T>> observable;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RealtimeDatabase{T}"/> class.
        /// </summary>
        /// <param name="childQuery"> The child query.  </param>
        /// <param name="elementRoot"> The element Root. </param>
        /// <param name="offlineDatabaseFactory"> The offline database factory.  </param>
        /// <param name="filenameModifier"> Custom string which will get appended to the file name.  </param>
        /// <param name="streamChanges"> Specifies whether changes should be streamed from the server.  </param>
        /// <param name="pullEverythingOnStart"> Specifies if everything should be pull from the online storage on start. It only makes sense when <see cref="streamChanges"/> is set to true. </param>
        /// <param name="pushChanges"> Specifies whether changed items should actually be pushed to the server. It this is false, then Put / Post / Delete will not affect server data. </param>
        public RealtimeDatabase(ChildQuery childQuery, string elementRoot, Func<Type, string, IDictionary<string, OfflineEntry>> offlineDatabaseFactory, string filenameModifier, bool streamChanges, InitialPullStrategy initialPullStrategy, bool pushChanges)
        {
            this.childQuery = childQuery;
            this.elementRoot = elementRoot;
            this.streamChanges = streamChanges;
            this.initialPullStrategy = initialPullStrategy;
            this.pushChanges = pushChanges;
            this.Database = offlineDatabaseFactory(typeof(T), filenameModifier);
            this.subject = new Subject<FirebaseEvent<T>>();

            this.PutHandler = new PutHandler<T>();

            Task.Factory.StartNew(this.SynchronizeThread, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Event raised whenever an exception is thrown in the synchronization thread. Exception thrown in there are swallowed, so this event is the only way to get to them. 
        /// </summary>
        public event EventHandler<ExceptionEventArgs> SyncExceptionThrown;

        /// <summary>
        /// Gets the backing Database.
        /// </summary>
        public IDictionary<string, OfflineEntry> Database
        {
            get;
            private set;
        }

        public IPutHandler<T> PutHandler
        {
            private get;
            set;
        }

        /// <summary>
        /// Overwrites existing object with given key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="obj"> The object to set. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param>
        public void Put(string key, T obj, bool syncOnline = true, int priority = 1)
        {
            this.SetAndRaise(key, new OfflineEntry(key, obj, priority, syncOnline ? SyncOptions.Push : SyncOptions.None));
        }

        /// <summary>
        /// Adds a new entity to the Database.
        /// </summary>
        /// <param name="obj"> The object to add.  </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param>
        /// <returns> The generated key for this object. </returns>
        public string Post(T obj, bool syncOnline = true, int priority = 1)
        {
            var key = FirebaseKeyGenerator.Next();

            this.SetAndRaise(key, new OfflineEntry(key, obj, priority, syncOnline ? SyncOptions.Push : SyncOptions.None));

            return key;
        }

        /// <summary>
        /// Deletes the entity with the given key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="syncOnline"> Indicates whether the item should be synced online. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param> 
        public void Delete(string key, bool syncOnline = true, int priority = 1)
        {
            this.SetAndRaise(key, new OfflineEntry(key, null, priority, syncOnline ? SyncOptions.Push : SyncOptions.None));
        }

        /// <summary>
        /// Fetches an object with the given key and adds it to the Database.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="priority"> The priority. Objects with higher priority will be synced first. Higher number indicates higher priority. </param>
        public void Pull(string key, int priority = 1)
        {
            if (!this.Database.ContainsKey(key))
            {
                this.Database[key] = new OfflineEntry(key, null, priority, SyncOptions.Pull);
            }
            else
            {
                this.Database[key].SyncOptions = SyncOptions.Pull;
            }
        }

        /// <summary> 
        /// Starts observing the real-time Database. Events will be fired both when change is done locally and remotely.
        /// </summary> 
        /// <returns> Stream of <see cref="FirebaseEvent{T}"/>. </returns>
        public IObservable<FirebaseEvent<T>> AsObservable()
        {
            if (this.observable == null)
            { 
                var initialData = this.Database.Count == 0 
                    ? Observable.Return(FirebaseEvent<T>.Empty(FirebaseEventSource.Offline)) 
                    : this.Database
                        .Where(kvp => !string.IsNullOrEmpty(kvp.Value.Data) && kvp.Value.Data != "null")
                        .Select(kvp => new FirebaseEvent<T>(kvp.Key, kvp.Value.Deserialize<T>(), FirebaseEventType.InsertOrUpdate, FirebaseEventSource.Offline))
                        .ToList()
                        .ToObservable();

                this.observable = initialData
                    .Merge(this.subject)
                    .Merge(this.GetInitialPullObservable()
                            .RetryAfterDelay(TimeSpan.FromSeconds(10))
                            .SelectMany(e => e)
                            .Do(e => this.Database[e.Key] = new OfflineEntry(e.Key, e.Object, 1, SyncOptions.None))
                            .Select(e => new FirebaseEvent<T>(e.Key, e.Object, FirebaseEventType.InsertOrUpdate, FirebaseEventSource.Online))
                            .Concat(Observable.Create<FirebaseEvent<T>>(observer => this.InitializeStreamingSubscription(observer))))
                    .Replay()
                    .RefCount();
            }

            return this.observable;
        }

        private IObservable<IReadOnlyCollection<FirebaseObject<T>>> GetInitialPullObservable()
        {
            FirebaseQuery query;
            switch (this.initialPullStrategy)
            {
                case InitialPullStrategy.MissingOnly:
                    query = this.childQuery.OrderByKey().StartAt(() => this.GetLatestKey());
                    break;
                case InitialPullStrategy.Everything:
                    query = this.childQuery;
                    break;
                case InitialPullStrategy.None:
                default:
                    return Observable.Empty<IReadOnlyCollection<FirebaseEvent<T>>>();
            }

            if (string.IsNullOrWhiteSpace(this.elementRoot))
            {
                return Observable.Defer(() => query.OnceAsync<T>().ToObservable());
            }
            
            // there is an element root, which indicates the target location is not a collection but a single element
            return Observable.Defer(async () => Observable.Return(await query.OnceSingleAsync<T>()).Select(e => new[] { new FirebaseObject<T>(this.elementRoot, e) }));
        }

        private IDisposable InitializeStreamingSubscription(IObserver<FirebaseEvent<T>> observer)
        {
            if (this.streamChanges)
            {
                var query = this.childQuery.OrderByKey().StartAt(() => this.GetLatestKey());
                var sub = new FirebaseSubscription<T>(observer, query, this.elementRoot, new FirebaseCache<T>(new OfflineCacheAdapter<string, T>(this.Database)));
                sub.ExceptionThrown += this.StreamingExceptionThrown;

                return sub.Run();
            } 
                
            return Observable.Never<string>().Subscribe();
        }

        private void SetAndRaise(string key, OfflineEntry obj, FirebaseEventSource eventSource = FirebaseEventSource.Offline)
        {
            this.Database[key] = obj;
            this.subject.OnNext(new FirebaseEvent<T>(key, obj?.Deserialize<T>(), string.IsNullOrEmpty(obj?.Data) || obj?.Data == "null" ? FirebaseEventType.Delete : FirebaseEventType.InsertOrUpdate, eventSource));
        }

        private async void SynchronizeThread()
        {
            while (true)
            {
                try
                {
                    var validEntries = this.Database.Where(e => e.Value != null);
                    await this.PullEntriesAsync(validEntries.Where(kvp => kvp.Value.SyncOptions == SyncOptions.Pull));

                    if (this.pushChanges)
                    {
                        await this.PushEntriesAsync(validEntries.Where(kvp => kvp.Value.SyncOptions == SyncOptions.Push));
                    }
                }
                catch (Exception ex)
                {
                    this.SyncExceptionThrown?.Invoke(this, new ExceptionEventArgs(ex));
                }

                await Task.Delay(10000);
            }
        }

        private string GetLatestKey()
        {
            return this.Database.OrderBy(o => o.Key, StringComparer.Ordinal).LastOrDefault().Key;
        }

        private async Task PushEntriesAsync(IEnumerable<KeyValuePair<string, OfflineEntry>> pushEntries)
        {
            var groups = pushEntries.GroupBy(pair => pair.Value.Priority).OrderByDescending(kvp => kvp.Key);

            foreach (var group in groups)
            {
                var tasks = group.Select(kvp => 
                    this.PutHandler
                        .PutAsync(this.childQuery, kvp.Key, kvp.Value)
                        .ContinueWith(t => this.ResetSyncOptions(kvp.Key), TaskContinuationOptions.OnlyOnRanToCompletion));

                try
                {
                    await Task.WhenAll(tasks).WithAggregateException();
                }
                catch (Exception ex)
                {
                    this.SyncExceptionThrown?.Invoke(this, new ExceptionEventArgs(ex));
                }
            }
        }

        private async Task PullEntriesAsync(IEnumerable<KeyValuePair<string, OfflineEntry>> pullEntries)
        {
            var taskGroups = pullEntries.GroupBy(pair => pair.Value.Priority).OrderByDescending(kvp => kvp.Key);

            foreach (var group in taskGroups)
            {
                var tasks = group.Select(pair => 
                    this.childQuery
                        .Child(pair.Key)
                        .OnceSingleAsync<T>()
                        .ContinueWith(
                            task => this.SetAndRaise(pair.Key, new OfflineEntry(pair.Key, task.Result, pair.Value.Priority, SyncOptions.None), FirebaseEventSource.Online),
                            TaskContinuationOptions.OnlyOnRanToCompletion));
                
                try
                { 
                    await Task.WhenAll(tasks).WithAggregateException();
                }
                catch (Exception ex)
                {
                    this.SyncExceptionThrown?.Invoke(this, new ExceptionEventArgs(ex));
                }
            }
        }

        private void ResetSyncOptions(string key)
        {
            var item = this.Database[key];
            item.SyncOptions = SyncOptions.None;
            this.Database[key] = item;
        }

        private void StreamingExceptionThrown(object sender, ExceptionEventArgs<FirebaseException> e)
        {
            this.SyncExceptionThrown?.Invoke(this, new ExceptionEventArgs(e.Exception));
        }
    }
}
