using System;
using System.Net.Http;

namespace Firebase
{
    internal sealed class TransientHttpClientFactory : IHttpClientFactory
    {
        public IHttpClientProxy GetHttpClient(TimeSpan? timeout)
        {
            var client = new HttpClient();
            if (timeout != null) {
                client.Timeout = timeout.Value;
            }

            return new SimpleHttpClientProxy(client);
        }
    }
}
