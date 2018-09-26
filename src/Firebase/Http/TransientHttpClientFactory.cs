using System;
using System.Net.Http;

namespace Firebase
{
    internal sealed class TransientHttpClientFactory : IHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            return new HttpClient();
        }
    }
}
