using System;
using System.Net.Http;

namespace Firebase
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }
}
