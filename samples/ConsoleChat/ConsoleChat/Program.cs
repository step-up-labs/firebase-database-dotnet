namespace Firebase.ConsoleChat
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using Firebase.Database;
    using Firebase.Database.Query;

    public class Program
    {
        public static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            Console.WriteLine("You can run this application multiple times to simulate chat between multiple users");
            Console.WriteLine("What's your name?");

            var name = Console.ReadLine();

            Console.WriteLine("*******************************************************");

            var client = new FirebaseClient("https://yourfirebase.firebaseio.com/");
            var child = client.Child("messages");
            
            var observable = child.AsObservable<InboundMessage>();
            
            // delete entire conversation list
            await child.DeleteAsync();

            // subscribe to messages comming in, ignoring the ones that are from me
            var subscription = observable
                .Where(f => !string.IsNullOrEmpty(f.Key)) // you get empty Key when there are no data on the server for specified node
                .Where(f => f.Object?.Author != name)
                .Subscribe(f => Console.WriteLine($"{f.Object.Author}: {f.Object.Content}"));

            while (true)
            {
                var message = Console.ReadLine();

                if (message?.ToLower() == "q")
                {
                    break;
                }

                await child.PostAsync(new OutboundMessage { Author = name, Content = message });
            }

            subscription.Dispose();
        }
    }
}