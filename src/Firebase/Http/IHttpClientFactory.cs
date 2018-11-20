using System;

namespace Firebase
{
    public interface IHttpClientFactory
    {
        IHttpClientProxy GetHttpClient(TimeSpan? timeout);
    }
}
