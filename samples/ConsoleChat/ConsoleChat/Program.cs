namespace Firebase.ConsoleChat
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public class Program
    {
        public static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            Console.WriteLine("What's your name?");

            var name = Console.ReadLine();

            Console.WriteLine("*******************************************************");

            var client = new FirebaseClient("https://yourfirebase.firebaseio.com/");
            var child = client.Child("messages");
            var o = child.AsObservable<Message>();
            
            // delete entire conversation list
            await child.DeleteAsync();

            // subscribe to messages comming in, ignoring the ones that are from me
            var subscription = o
                .Where(f => f.Object.Author != name)
                .Subscribe(m => Console.WriteLine($"{m.Object.Author}: {m.Object.Content}"));

            while (true)
            {
                var message = Console.ReadLine();

                if (message?.ToLower() == "q")
                {
                    break;
                }

                await child.PostAsync(new Message { Author = name, Content = message });
            }

            subscription.Dispose();
        }
    }
}