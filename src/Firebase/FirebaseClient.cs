using System.Net.Http;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Firebase.Database.Tests")]

namespace Firebase.Database
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Firebase.Database.Offline;
    using Firebase.Database.Query;

    /// <summary>
    /// Firebase client which acts as an entry point to the online database.
    /// </summary>
    public class FirebaseClient : IDisposable
    {
        internal readonly IHttpClientProxy HttpClient;
        internal readonly FirebaseOptions Options;
		private readonly string baseUrl;

		public readonly string ns;


        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseClient"/> class.
        /// </summary>
        /// <param name="baseUrl"> The base url. </param>
        /// <param name="options"> The Firebase options. </param>  
        public FirebaseClient(string baseUrl, FirebaseOptions options = null, string ns = null)
        {
            this.Options = options ?? new FirebaseOptions();
			this.HttpClient = Options.HttpClientFactory.GetHttpClient(null);

            this.baseUrl = baseUrl;
			this.ns = ns;

            if (!this.baseUrl.EndsWith("/"))
            {
                this.baseUrl += "/";
            }
        }

        /// <summary>
        /// Queries for a child of the data root.
        /// </summary>
        /// <param name="resourceName"> Name of the child. </param>
        /// <returns> <see cref="ChildQuery"/>. </returns>
        public ChildQuery Child(string resourceName)
        {
            return new ChildQuery(this, () => this.baseUrl + resourceName);
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}
