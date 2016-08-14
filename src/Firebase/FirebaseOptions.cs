namespace Firebase.Database
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Firebase.Database.Offline;
    using System.IO;

    public class FirebaseOptions
    {
        public FirebaseOptions()
        {
            this.OfflineDatabaseFactory = (t, s) => new Dictionary<string, OfflineEntry>();
            this.SubscriptionStreamReaderFactory = s => new StreamReader(s);
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
    }
}
