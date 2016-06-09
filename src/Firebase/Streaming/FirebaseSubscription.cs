namespace Firebase.Database.Streaming
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    using Firebase.Database.Query;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The firebase subscription.
    /// </summary>
    /// <typeparam name="T"> Type of object to be streaming back to the called. </typeparam>
    internal class FirebaseSubscription<T> : IDisposable
    {
        private readonly CancellationTokenSource cancel;
        private readonly HttpClient httpClient;
        private readonly IObserver<FirebaseEvent<T>> observer;
        private readonly IFirebaseQuery query;
        private readonly FirebaseCache<T> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseSubscription{T}"/> class.
        /// </summary>
        /// <param name="observer"> The observer. </param>
        /// <param name="query"> The query. </param>
        public FirebaseSubscription(IObserver<FirebaseEvent<T>> observer, IFirebaseQuery query)
        {
            this.observer = observer;
            this.query = query;
            this.cancel = new CancellationTokenSource();
            this.httpClient = new HttpClient();
            this.cache = new FirebaseCache<T>();

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10,
            };

            this.httpClient = new HttpClient(handler, true)
            {
                Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite),
            };

            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
            this.cancel.Cancel();
        }

        public IDisposable Run()
        {
            Task.Factory.StartNew(this.ReceiveThread);

            return this;
        }

        private async void ReceiveThread()
        {
            while (true)
            {
                try
                {
                    this.cancel.Token.ThrowIfCancellationRequested();

                    // initialize network connection
                    var serverEvent = FirebaseServerEventType.KeepAlive;
                    var request = new HttpRequestMessage(HttpMethod.Get, await this.query.BuildUrlAsync());
                    var response = await this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var reader = new StreamReader(stream))
                    {
                        while (true)
                        {
                            this.cancel.Token.ThrowIfCancellationRequested();

                            var line = reader.ReadLine();

                            if (string.IsNullOrWhiteSpace(line))
                            {
                                await Task.Delay(2000);
                                continue;
                            }

                            var tuple = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                            switch (tuple[0].ToLower())
                            {
                                case "event":
                                    serverEvent = this.ParseServerEvent(serverEvent, tuple[1]);
                                    break;
                                case "data":
                                    this.ProcessServerData(serverEvent, tuple[1]);
                                    break;
                            }

                            if (serverEvent == FirebaseServerEventType.AuthRevoked)
                            {
                                // auth token no longer valid, reconnect
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("************************************************************");
                    Debug.WriteLine(ex.ToString());
                    Debug.WriteLine("************************************************************");
                    await Task.Delay(2000);
                }
            }
        }

        private FirebaseServerEventType ParseServerEvent(FirebaseServerEventType serverEvent, string eventName)
        {
            switch (eventName)
            {
                case "put":
                    serverEvent = FirebaseServerEventType.Put;
                    break;
                case "patch":
                    serverEvent = FirebaseServerEventType.Patch;
                    break;
                case "keep-alive":
                    serverEvent = FirebaseServerEventType.KeepAlive;
                    break;
                case "cancel":
                    serverEvent = FirebaseServerEventType.Cancel;
                    break;
                case "auth_revoked":
                    serverEvent = FirebaseServerEventType.AuthRevoked;
                    break;
            }

            return serverEvent;
        }

        private void ProcessServerData(FirebaseServerEventType serverEvent, string serverData)
        {
            switch (serverEvent)
            {
                case FirebaseServerEventType.Put:
                case FirebaseServerEventType.Patch:
                    var result = JObject.Parse(serverData);
                    var path = result["path"].ToString();
                    var data = result["data"].ToString();
                    var eventType = string.IsNullOrWhiteSpace(data) ? FirebaseEventType.Delete : FirebaseEventType.InsertOrUpdate;

                    var items = this.cache.PushData(path, data);

                    foreach (var i in items.ToList())
                    {
                        this.observer.OnNext(new FirebaseEvent<T>(i.Key, i.Object, eventType));
                    }

                    break;
                case FirebaseServerEventType.KeepAlive:
                    break;
                case FirebaseServerEventType.Cancel:
                    this.observer.OnError(new Exception("cancel"));
                    this.Dispose();
                    throw new OperationCanceledException();
                case FirebaseServerEventType.AuthRevoked:
                    this.observer.OnError(new Exception("auth"));
                    this.Dispose();
                    throw new OperationCanceledException();
            }
        }
    }
}
