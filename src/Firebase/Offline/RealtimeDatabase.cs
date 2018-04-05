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
    using System.Linq.Expressions;
    using Internals;
    using Newtonsoft.Json;
    using System.Reflection;
    using System.Reactive.Disposables;

    /// <summary>
    /// The real-time Database which synchronizes online and offline data. 
    /// </summary>
    /// <typeparam name="T"> Type of entities. </typeparam>
    public partial class RealtimeDatabase<T> : IDisposable where T : class
    {
        private readonly ChildQuery childQuery;
        private readonly string elementRoot;
        private readonly StreamingOptions streamingOptions;
        private readonly Subject<FirebaseEvent<T>> subject;
        private readonly InitialPullStrategy initialPullStrategy;
        private readonly bool pushChanges;
        private readonly FirebaseCache<T> firebaseCache;

        private bool isSyncRunning;
        private IObservable<FirebaseEvent<T>> observable;
        private FirebaseSubscription<T> firebaseSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealtimeDatabase{T}"/> class.
        /// </summary>
        /// <param name="childQuery"> The child query.  </param>
        /// <param name="elementRoot"> The element Root. </param>
        /// <param name="offlineDatabaseFactory"> The offline database factory.  </param>
        /// <param name="filenameModifier"> Custom string which will get appended to the file name.  </param>
        /// <param name="streamChanges"> Specifies whether changes should be streamed from the server.  </param>
        /// <param name="pullEverythingOnStart"> Specifies if everything should be pull from the online storage on start. It only makes sense when <see cref="streamChanges"/> is set to true. </param>
        /// <param name="pushChanges"> Specifies whether changed items should actually be pushed to the server. If this is false, then Put / Post / Delete will not affect server data. </param>
        public RealtimeDatabase(ChildQuery childQuery, string elementRoot, Func<Type, string, IDictionary<string, OfflineEntry>> offlineDatabaseFactory, string filenameModifier, StreamingOptions streamingOptions, InitialPullStrategy initialPullStrategy, bool pushChanges, ISetHandler<T> setHandler = null)
        {
            this.childQuery = childQuery;
            this.elementRoot = elementRoot;
            this.streamingOptions = streamingOptions;
            this.initialPullStrategy = initialPullStrategy;
            this.pushChanges = pushChanges;
            this.Database = offlineDatabaseFactory(typeof(T), filenameModifier);
            this.firebaseCache = new FirebaseCache<T>(new OfflineCacheAdapter<string, T>(this.Database));
            this.subject = new Subject<FirebaseEvent<T>>();

            this.PutHandler = setHandler ?? new SetHandler<T>();

            this.isSyncRunning = true;
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

        public ISetHandler<T> PutHandler
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
        public void Set(string key, T obj, SyncOptions syncOptions, int priority = 1)
        {
            this.SetAndRaise(key, new OfflineEntry(key, obj, priority, syncOptions));
        }

        public void Set<TProperty>(string key, Expression<Func<T, TProperty>> propertyExpression, object value, SyncOptions syncOptions, int priority = 1)
        {
            var fullKey = this.GenerateFullKey(key, propertyExpression, syncOptions);
            var serializedObject = JsonConvert.SerializeObject(value).Trim('"', '\\');

            if (fullKey.Item3)
            {
                if (typeof(TProperty) != typeof(string) || value == null)
                {
                    // don't escape non-string primitives and null;
                    serializedObject = $"{{ \"{fullKey.Item2}\" : {serializedObject} }}";
                }
                else
                {
                    serializedObject = $"{{ \"{fullKey.Item2}\" : \"{serializedObject}\" }}";
                }
            }

            var setObject = this.firebaseCache.PushData("/" + fullKey.Item1, serializedObject).First();

            if (!this.Database.ContainsKey(key) || this.Database[key].SyncOptions != SyncOptions.Patch && this.Database[key].SyncOptions != SyncOptions.Put)
            {
                this.Database[fullKey.Item1] = new OfflineEntry(fullKey.Item1, value, serializedObject, priority, syncOptions, true);
            }

            this.subject.OnNext(new FirebaseEvent<T>(key, setObject.Object, setObject == null ? FirebaseEventType.Delete : FirebaseEventType.InsertOrUpdate, FirebaseEventSource.Offline));
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
            else if (this.Database[key].SyncOptions == SyncOptions.None)
            {
                // pull only if push isn't pending
                this.Database[key].SyncOptions = SyncOptions.Pull;
            }
        }

        /// <summary>
        /// Fetches everything from the remote database.
        /// </summary>
        public async Task PullAsync()
        {
            var existingEntries = await this.childQuery
                .OnceAsync<T>()
                .ToObservable()
                .RetryAfterDelay<IReadOnlyCollection<FirebaseObject<T>>, FirebaseException>(
                    this.childQuery.Client.Options.SyncPeriod,
                    ex => ex.StatusCode == System.Net.HttpStatusCode.OK) // OK implies the request couldn't complete due to network error. 
                .SelectMany(e => e)
                .Do(e => 
                {
                    this.Database[e.Key] = new OfflineEntry(e.Key, e.Object, 1, SyncOptions.None);
                    this.subject.OnNext(new FirebaseEvent<T>(e.Key, e.Object, FirebaseEventType.InsertOrUpdate, FirebaseEventSource.OnlinePull));
                })
                .ToList();

            // Remove items not stored online
            foreach (var item in this.Database.Keys.Except(existingEntries.Select(f => f.Key)).ToList())
            {
                this.Database.Remove(item);
                this.subject.OnNext(new FirebaseEvent<T>(item, null, FirebaseEventType.Delete, FirebaseEventSource.OnlinePull));
            }
        }

        /// <summary>
        /// Retrieves all offline items currently stored in local database.
        /// </summary>
        public IEnumerable<FirebaseObject<T>> Once()
        {
            return this.Database
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value.Data) && kvp.Value.Data != "null" && !kvp.Value.IsPartial)
                .Select(kvp => new FirebaseObject<T>(kvp.Key, kvp.Value.Deserialize<T>()))
                .ToList();
        }

        /// <summary> 
        /// Starts observing the real-time Database. Events will be fired both when change is done locally and remotely.
        /// </summary> 
        /// <returns> Stream of <see cref="FirebaseEvent{T}"/>. </returns>
        public IObservable<FirebaseEvent<T>> AsObservable()
        {
            if (!this.isSyncRunning)
            {
                this.isSyncRunning = true;
                Task.Factory.StartNew(this.SynchronizeThread, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            if (this.observable == null)
            {
                var initialData = this.Database.Count == 0
                    ? Observable.Return(FirebaseEvent<T>.Empty(FirebaseEventSource.Offline))
                    : this.Database
                        .Where(kvp => !string.IsNullOrEmpty(kvp.Value.Data) && kvp.Value.Data != "null" && !kvp.Value.IsPartial)
                        .Select(kvp => new FirebaseEvent<T>(kvp.Key, kvp.Value.Deserialize<T>(), FirebaseEventType.InsertOrUpdate, FirebaseEventSource.Offline))
                        .ToList()
                        .ToObservable();

                this.observable = initialData
                    .Merge(this.subject)
                    .Merge(this.GetInitialPullObservable()
                            .RetryAfterDelay<IReadOnlyCollection<FirebaseObject<T>>, FirebaseException>(
                                this.childQuery.Client.Options.SyncPeriod, 
                                ex => ex.StatusCode == System.Net.HttpStatusCode.OK) // OK implies the request couldn't complete due to network error. 
                            .SelectMany(e => e)
                            .Do(this.SetObjectFromInitialPull)
                            .Select(e => new FirebaseEvent<T>(e.Key, e.Object, FirebaseEventType.InsertOrUpdate, FirebaseEventSource.OnlineInitial))
                            .Concat(Observable.Create<FirebaseEvent<T>>(observer => this.InitializeStreamingSubscription(observer))))
                            .Do(next => { }, e => this.observable = null, () => this.observable = null)
                    .Replay()
                    .RefCount();
            }

            return this.observable;
        }

        public void Dispose()
        {
            this.subject.OnCompleted();
            this.firebaseSubscription?.Dispose();
        }

        private void SetObjectFromInitialPull(FirebaseObject<T> e)
        {
            // set object with no sync only if it doesn't exist yet 
            // and the InitialPullStrategy != Everything
            // this attempts to deal with scenario when you are offline, have local changes and go online
            // in this case having the InitialPullStrategy set to everything would basically purge all local changes
            if (!this.Database.ContainsKey(e.Key) || this.Database[e.Key].SyncOptions == SyncOptions.None || this.Database[e.Key].SyncOptions == SyncOptions.Pull || this.initialPullStrategy != InitialPullStrategy.Everything)
            {
                this.Database[e.Key] = new OfflineEntry(e.Key, e.Object, 1, SyncOptions.None);
            }
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
            var completeDisposable = Disposable.Create(() => this.isSyncRunning = false);

            switch (this.streamingOptions)
            {
                case StreamingOptions.LatestOnly:
                    // stream since the latest key
                    var queryLatest = this.childQuery.OrderByKey().StartAt(() => this.GetLatestKey());
                    this.firebaseSubscription = new FirebaseSubscription<T>(observer, queryLatest, this.elementRoot, this.firebaseCache);
                    this.firebaseSubscription.ExceptionThrown += this.StreamingExceptionThrown;

                    return new CompositeDisposable(this.firebaseSubscription.Run(), completeDisposable);
                case StreamingOptions.Everything:
                    // stream everything
                    var queryAll = this.childQuery;
                    this.firebaseSubscription = new FirebaseSubscription<T>(observer, queryAll, this.elementRoot, this.firebaseCache);
                    this.firebaseSubscription.ExceptionThrown += this.StreamingExceptionThrown;

                    return new CompositeDisposable(this.firebaseSubscription.Run(), completeDisposable);
                default:
                    break;
            }

            return completeDisposable;
        }

        private void SetAndRaise(string key, OfflineEntry obj, FirebaseEventSource eventSource = FirebaseEventSource.Offline)
        {
            this.Database[key] = obj;
            this.subject.OnNext(new FirebaseEvent<T>(key, obj?.Deserialize<T>(), string.IsNullOrEmpty(obj?.Data) || obj?.Data == "null" ? FirebaseEventType.Delete : FirebaseEventType.InsertOrUpdate, eventSource));
        }

        private async void SynchronizeThread()
        {
            while (this.isSyncRunning)
            {
                try
                {
                    var validEntries = this.Database.Where(e => e.Value != null);
                    await this.PullEntriesAsync(validEntries.Where(kvp => kvp.Value.SyncOptions == SyncOptions.Pull));

                    if (this.pushChanges)
                    {
                        await this.PushEntriesAsync(validEntries.Where(kvp => kvp.Value.SyncOptions == SyncOptions.Put || kvp.Value.SyncOptions == SyncOptions.Patch));
                    }
                }
                catch (Exception ex)
                {
                    this.SyncExceptionThrown?.Invoke(this, new ExceptionEventArgs(ex));
                }

                await Task.Delay(this.childQuery.Client.Options.SyncPeriod);
            }
        }

        private string GetLatestKey()
        {
            var key = this.Database.OrderBy(o => o.Key, StringComparer.Ordinal).LastOrDefault().Key ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Substring(0, key.Length - 1) + (char)(key[key.Length - 1] + 1);
            }

            return key;
        }

        private async Task PushEntriesAsync(IEnumerable<KeyValuePair<string, OfflineEntry>> pushEntries)
        {
            var groups = pushEntries.GroupBy(pair => pair.Value.Priority).OrderByDescending(kvp => kvp.Key).ToList();

            foreach (var group in groups)
            {
                var tasks = group.OrderBy(kvp => kvp.Value.IsPartial).Select(kvp => 
                    kvp.Value.IsPartial ?
                    this.ResetSyncAfterPush(this.PutHandler.SetAsync(this.childQuery, kvp.Key, kvp.Value), kvp.Key) :
                    this.ResetSyncAfterPush(this.PutHandler.SetAsync(this.childQuery, kvp.Key, kvp.Value), kvp.Key, kvp.Value.Deserialize<T>()));

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
                var tasks = group.Select(pair => this.ResetAfterPull(this.childQuery.Child(pair.Key == this.elementRoot ? string.Empty : pair.Key).OnceSingleAsync<T>(), pair.Key, pair.Value));

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

        private async Task ResetAfterPull(Task<T> task, string key, OfflineEntry entry)
        {
            await task;
            this.SetAndRaise(key, new OfflineEntry(key, task.Result, entry.Priority, SyncOptions.None), FirebaseEventSource.OnlinePull);
        }

        private async Task ResetSyncAfterPush(Task task, string key, T obj)
        {
            await this.ResetSyncAfterPush(task, key);

            if (this.streamingOptions == StreamingOptions.None)
            {
                this.subject.OnNext(new FirebaseEvent<T>(key, obj, obj == null ? FirebaseEventType.Delete : FirebaseEventType.InsertOrUpdate, FirebaseEventSource.OnlinePush));
            }
        }

        private async Task ResetSyncAfterPush(Task task, string key)
        {
            await task;
            this.ResetSyncOptions(key);
        }

        private void ResetSyncOptions(string key)
        {
            var item = this.Database[key];

            if (item.IsPartial)
            {
                this.Database.Remove(key);
            }
            else
            {
                item.SyncOptions = SyncOptions.None;
                this.Database[key] = item;
            }
        }

        private void StreamingExceptionThrown(object sender, ExceptionEventArgs<FirebaseException> e)
        {
            this.SyncExceptionThrown?.Invoke(this, new ExceptionEventArgs(e.Exception));
        }

        private Tuple<string, string, bool> GenerateFullKey<TProperty>(string key, Expression<Func<T, TProperty>> propertyGetter, SyncOptions syncOptions)
        {
            var visitor = new MemberAccessVisitor();
            visitor.Visit(propertyGetter);
            var propertyType = typeof(TProperty).GetTypeInfo();
            var prefix = key == string.Empty ? string.Empty : key + "/";

            // primitive types
            if (syncOptions == SyncOptions.Patch && (propertyType.IsPrimitive || Nullable.GetUnderlyingType(typeof(TProperty)) != null || typeof(TProperty) == typeof(string)))
            {
                return Tuple.Create(prefix + string.Join("/", visitor.PropertyNames.Skip(1).Reverse()), visitor.PropertyNames.First(), true);
            }

            return Tuple.Create(prefix + string.Join("/", visitor.PropertyNames.Reverse()), visitor.PropertyNames.First(), false);
        }

    }
}
