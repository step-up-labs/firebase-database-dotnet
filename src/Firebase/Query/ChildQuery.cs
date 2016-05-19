namespace Firebase.Query
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// Firebase query which references the child of current node.
    /// </summary>
    public class ChildQuery : FirebaseQuery, IDisposable
    {
        private readonly string path;
        private HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildQuery"/> class.
        /// </summary>
        /// <param name="path"> The path to the child node. </param>
        /// <param name="parent"> The parent. </param>
        public ChildQuery(string path, FirebaseQuery parent)
            : base(parent)
        {
            this.path = path;

            if (!this.path.EndsWith("/"))
            {
                this.path += "/";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildQuery"/> class.
        /// </summary>
        /// <param name="path"> The path to the child node. </param>
        public ChildQuery(string path)
            : this(path, null)
        {
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }

        public async Task<FirebaseObject<T>> PostAsync<T>(T obj)
        {
            var key = FirebaseKeyGenerator.Next();
            var c = this.GetClient();

            await new ChildQuery(key, this).SendAsync(c, obj, HttpMethod.Post);

            return new FirebaseObject<T>(key, obj);
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
        /// <param name="child"> The child of this child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlSegment(FirebaseQuery child)
        {
            if (!(child is ChildQuery))
            {
                return this.path + ".json";
            }

            return this.path;
        }

        private async Task SendAsync<T>(HttpClient client, T obj, HttpMethod method)
        {
            var url = this.BuildUrl();
            var message = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(obj))
            };

            var result = await client.SendAsync(message);

            result.EnsureSuccessStatusCode();
        }

        private HttpClient GetClient()
        {
            if (this.client == null)
            {
                this.client = new HttpClient();
            }

            return this.client;
        }
    }
}
