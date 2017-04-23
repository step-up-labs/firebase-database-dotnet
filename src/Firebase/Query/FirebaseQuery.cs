namespace Firebase.Database.Query
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using Firebase.Database.Http;
    using Firebase.Database.Offline;
    using Firebase.Database.Streaming;

    using Newtonsoft.Json;
    using System.Net;

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
        /// <param name="client"> The owning client. </param>
        protected FirebaseQuery(FirebaseQuery parent, FirebaseClient client)
        {
            this.Client = client;
            this.Parent = parent;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        public FirebaseClient Client
        {
            get;
        }

        /// <summary>
        /// Queries the firebase server once returning collection of items.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <returns> Collection of <see cref="FirebaseObject{T}"/> holding the entities returned by server. </returns>
        public async Task<IReadOnlyCollection<FirebaseObject<T>>> OnceAsync<T>()
        {
            var path = await this.BuildUrlAsync().ConfigureAwait(false);

            return await this.GetClient().GetObjectCollectionAsync<T>(path).ConfigureAwait(false);
        }


        /// <summary>
        /// Assumes given query is pointing to a single object of type <typeparamref name="T"/> and retrieves it.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <returns> Single object of type <typeparamref name="T"/>. </returns>
        public async Task<T> OnceSingleAsync<T>()
        {
            var path = await this.BuildUrlAsync().ConfigureAwait(false);
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                var response = await this.GetClient().GetAsync(path).ConfigureAwait(false);
                statusCode = response.StatusCode;
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<T>(responseData);
            }
            catch(Exception ex)
            {
                throw new FirebaseException(path, string.Empty, responseData, statusCode, ex);
            }
        }

        /// <summary>
        /// Starts observing this query watching for changes real time sent by the server.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <param name="elementRoot"> Optional custom root element of received json items. </param>
        /// <returns> Observable stream of <see cref="FirebaseEvent{T}"/>. </returns>
        public IObservable<FirebaseEvent<T>> AsObservable<T>(EventHandler<ExceptionEventArgs<FirebaseException>> exceptionHandler = null, string elementRoot = "")
        {
            return Observable.Create<FirebaseEvent<T>>(observer =>
            {
                var sub = new FirebaseSubscription<T>(observer, this, elementRoot, new FirebaseCache<T>());
                sub.ExceptionThrown += exceptionHandler;
                return sub.Run();
            });
        }

        /// <summary>
        /// Builds the actual URL of this query.
        /// </summary>
        /// <returns> The <see cref="string"/>. </returns>
        public async Task<string> BuildUrlAsync()
        {
            // if token factory is present on the parent then use it to generate auth token
            if (this.Client.Options.AuthTokenAsyncFactory != null)
            {
                var token = await this.Client.Options.AuthTokenAsyncFactory().ConfigureAwait(false);
                return this.WithAuth(token).BuildUrl(null);
            }

            return this.BuildUrl(null);
        }

        /// <summary>
        /// Posts given object to repository.
        /// </summary>
        /// <param name="obj"> The object. </param> 
        /// <param name="generateKeyOffline"> Specifies whether the key should be generated offline instead of online. </param> 
        /// <typeparam name="T"> Type of <see cref="obj"/> </typeparam>
        /// <returns> Resulting firebase object with populated key. </returns>
        public async Task<FirebaseObject<string>> PostAsync(string data, bool generateKeyOffline = true)
        {
            // post generates a new key server-side, while put can be used with an already generated local key
            if (generateKeyOffline)
            {
                var key = FirebaseKeyGenerator.Next();
                await new ChildQuery(this, () => key, this.Client).PutAsync(data).ConfigureAwait(false);

                return new FirebaseObject<string>(key, data);
            }
            else
            {
                var c = this.GetClient();
                var sendData = await this.SendAsync(c, data, HttpMethod.Post).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<PostResult>(sendData);

                return new FirebaseObject<string>(result.Name, data);
            }
        }

        /// <summary>
        /// Patches data at given location instead of overwriting them. 
        /// </summary> 
        /// <param name="obj"> The object. </param>  
        /// <typeparam name="T"> Type of <see cref="obj"/> </typeparam>
        /// <returns> The <see cref="Task"/>. </returns>
        public async Task PatchAsync(string data)
        {
            var c = this.GetClient();

            await this.SendAsync(c, data, new HttpMethod("PATCH")).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets or overwrites data at given location. 
        /// </summary> 
        /// <param name="obj"> The object. </param>  
        /// <typeparam name="T"> Type of <see cref="obj"/> </typeparam>
        /// <returns> The <see cref="Task"/>. </returns>
        public async Task PutAsync(string data)
        {
            var c = this.GetClient();

            await this.SendAsync(c, data, HttpMethod.Put).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes data from given location.
        /// </summary>
        /// <returns> The <see cref="Task"/>. </returns>
        public async Task DeleteAsync()
        {
            var c = this.GetClient();
            var url = await this.BuildUrlAsync().ConfigureAwait(false);
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                var result = await c.DeleteAsync(url).ConfigureAwait(false);
                statusCode = result.StatusCode;
                responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new FirebaseException(url, string.Empty, responseData, statusCode, ex);
            }
        }

        /// <summary>
        /// Disposes this instance.  
        /// </summary>
        public void Dispose()
        {
            this.client?.Dispose();
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

        private async Task<string> SendAsync(HttpClient client, string data, HttpMethod method)
        {
            var url = await this.BuildUrlAsync().ConfigureAwait(false);
            var requestData = data;
            var message = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(requestData)
            };

            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                var result = await client.SendAsync(message).ConfigureAwait(false);
                statusCode = result.StatusCode;
                responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return responseData;
            }
            catch(Exception ex)
            {
                throw new FirebaseException(url, requestData, responseData, statusCode, ex);
            }
        }
    }
}
