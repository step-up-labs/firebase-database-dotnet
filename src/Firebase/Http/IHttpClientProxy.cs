using System;
using System.Net.Http;

namespace Firebase
{
    public interface IHttpClientProxy : IDisposable
    {
        HttpClient GetHttpClient();
    }
}
