namespace Firebase.Query
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using Firebase.Http;
    using Firebase.Streaming;

    /// <summary>
    /// Represents a firebase query. 
    /// </summary>
    public abstract class FirebaseQuery : IFirebaseQuery
    {
        protected readonly FirebaseQuery Parent;
         
        /// <summary> 
        /// Initializes a new instance of the <see cref="FirebaseQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent of this query. </param>
        protected FirebaseQuery(FirebaseQuery parent)
        {
            this.Parent = parent;
        }

        /// <summary>
        /// Queries the firebase server once returning collection of items.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <returns> Collection of <see cref="FirebaseObject{T}"/> holding the entities returned by server. </returns>
        public async Task<IReadOnlyCollection<FirebaseObject<T>>> OnceAsync<T>()
        {
            var path = this.BuildUrl();

            using (var client = new HttpClient())
            {
                return await client.GetObjectCollectionAsync<T>(path);
            }
        }

        /// <summary>
        /// Starts observing this query watching for changes real time sent by the server.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <returns> Observable stream of <see cref="FirebaseEvent{T}"/>. </returns>
        public IObservable<FirebaseEvent<T>> AsObservable<T>()
        {
            return Observable.Create<FirebaseEvent<T>>(observer => new FirebaseSubscription<T>(observer, this).Run());
        }

        /// <summary>
        /// Builds the actual URL of this query.
        /// </summary>
        /// <returns> The <see cref="string"/>. </returns>
        public string BuildUrl()
        {
            return this.BuildUrl(null);
        }

        /// <summary>
        /// Build the url segment of this child.
        /// </summary>
        /// <param name="child"> The child of this query. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected abstract string BuildUrlSegment(FirebaseQuery child);

        private string BuildUrl(FirebaseQuery child)
        {
            var url = this.BuildUrlSegment(child);

            if (this.Parent != null)
            {
                url = this.Parent.BuildUrl(this) + url;
            }

            return url;
        }
    }
}
