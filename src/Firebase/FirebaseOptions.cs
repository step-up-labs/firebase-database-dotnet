namespace Firebase.Database
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Firebase.Database.Offline;

    using Newtonsoft.Json;

    public class FirebaseOptions
    {
        public FirebaseOptions()
        {
            this.OfflineDatabaseFactory = (t, s) => new Dictionary<string, OfflineEntry>();
            this.SubscriptionStreamReaderFactory = s => new StreamReader(s);
            this.JsonSerializerSettings = new JsonSerializerSettings();
            this.SyncPeriod = TimeSpan.FromSeconds(10);
            this.HttpClientFactory = new TransientHttpClientFactory();
        }

        /// <summary>
        /// Gets or sets the factory for Firebase offline database. Default is in-memory dictionary.
        /// </summary>
        public Func<Type, string, IDictionary<string, OfflineEntry>> OfflineDatabaseFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method for retrieving auth tokens. Default is null.
        /// </summary>
        public Func<Task<string>> AuthTokenAsyncFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the factory for <see cref="TextReader"/> used for reading online streams. Default is <see cref="StreamReader"/>.
        /// </summary>
        public Func<Stream, TextReader> SubscriptionStreamReaderFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the json serializer settings.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time between synchronization attempts for pulling and pushing offline entities. Default is 10 seconds.
        /// </summary>
        public TimeSpan SyncPeriod
        {
            get;
            set;
        }

        /// <summary>
        /// Specify if token returned by factory will be used as "auth" url parameter or "access_token". 
        /// </summary>
        public bool AsAccessToken
        {
            get;
            set;
        }

        /// <summary>
        /// Specify HttpClient factory to manage <see cref="System.Net.Http.HttpClient" /> lifecycle.
        /// </summary>
        public IHttpClientFactory HttpClientFactory {
            get;
            set;
        }
    }
}
