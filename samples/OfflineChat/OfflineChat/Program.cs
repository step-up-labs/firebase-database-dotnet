namespace Firebase.Database.OfflineChat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Firebase.Database.Offline;

    internal class Program 
    {
        public static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var client = new FirebaseClient(
                "https://yourfirebase.firebaseio.com/",
                new FirebaseOptions
                {
                    OfflineDatabaseFactory = (t, s) => new OfflineDatabase(t, s),
                    //AuthTokenAsyncFactory = () => Task.FromResult("<YOUR AUTH IF NEEDED>")
                });


            var messagesDb = client.Child("messages").AsRealtimeDatabase<Message>(string.Empty);
            var authorsDb = client.Child("authors").AsRealtimeDatabase<Author>(string.Empty);

            // watch exceptions
            authorsDb.SyncExceptionThrown += (s, ex) => Console.WriteLine(ex.Exception);
            messagesDb.SyncExceptionThrown += (s, ex) => Console.WriteLine(ex.Exception);

            Console.WriteLine("What's you name?");
            var name = Console.ReadLine();

            var myKey = authorsDb.Post(new Author { Name = name });

            var e = from author in authorsDb.AsObservable()
                    from message in messagesDb.AsObservable()
                    where author.Key == message.Object.Author
                    select new { author, message };

            e.Subscribe(pair => Console.WriteLine($"{pair.author.Object.Name}: {pair.message.Object.Content}"));

            while (true)
            {
                var message = Console.ReadLine();

                if (message?.ToLower() == "q")
                {
                    break;
                }

                messagesDb.Post(new Message { Author = myKey, Content = message });
            }
        }
    }
}
