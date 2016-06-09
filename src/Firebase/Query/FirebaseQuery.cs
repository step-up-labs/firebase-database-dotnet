namespace Firebase.Database.Query
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using Firebase.Database.Http;
    using Firebase.Database.Streaming;

    using Newtonsoft.Json;

    /// <summary>
    /// Represents a firebase query. 
    /// </summary>
    public abstract class FirebaseQuery : IFirebaseQuery, IDisposable
    {
        protected readonly FirebaseQuery Parent;
         
        private HttpClient client;

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

        public void Dispose()
        {
            this.client?.Dispose();
        }

        public async Task<FirebaseObject<T>> PostAsync<T>(T obj, bool generateKeyOffline = true)
        {
            // post generates a new key server-side, while put can be used with an already generated local key
            if (generateKeyOffline)
            {
                var key = FirebaseKeyGenerator.Next();
                await new ChildQuery(key, this).PutAsync(obj);

                return new FirebaseObject<T>(key, obj);
            }
            else
            {
                var c = this.GetClient();
                var data = await this.SendAsync(c, obj, HttpMethod.Post);
                var result = JsonConvert.DeserializeObject<PostResult>(data);

                return new FirebaseObject<T>(result.Name, obj);
            }
        }

        public async Task PutAsync<T>(T obj)
        {
            var c = this.GetClient();

            await this.SendAsync(c, obj, HttpMethod.Put);
        }

        public async Task DeleteAsync()
        {
            var c = this.GetClient();
            var url = this.BuildUrl();
            var result = await c.DeleteAsync(url);

            result.EnsureSuccessStatusCode();
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

        private HttpClient GetClient()
        {
            if (this.client == null)
            {
                this.client = new HttpClient();
            }

            return this.client;
        }

        private async Task<string> SendAsync<T>(HttpClient client, T obj, HttpMethod method)
        {
            var url = this.BuildUrl();
            var message = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(obj))
            };

            var result = await client.SendAsync(message);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStringAsync();
        }
    }
}
