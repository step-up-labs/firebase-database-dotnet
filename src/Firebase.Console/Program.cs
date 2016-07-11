namespace Firebase.Console
{
    using System;
    using System.Collections.Generic;
    using System.Data.Odbc;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using Firebase.Database;
    using Firebase.Database.Offline;
    using Firebase.Database.Query;
    using Firebase.Database.Tests.Entities;

    using Lex.Db;

    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run().Wait();
        }

        private async Task Run()
        {
            
            var firebase = new FirebaseClient(
                "https://torrid-inferno-3642.firebaseio.com/", 
                () => Task.FromResult("XgifPTGvB49oRQ4ArCse22M7j9kJfyHIb9GSEE9T"),
                (t, s)  => new OfflineDatabase(t, s));

            var dinos = firebase.Child("dinosaurs").AsRealtimeDatabase<Dinosaur>("a").AsObservable();

            await Task.Delay(1000);

            var d1 = dinos.Subscribe(d => Console.WriteLine("D1: " + d.Key));

            await Task.Delay(3000);

            var d2 = dinos.Subscribe(d => Console.WriteLine("D2: " + d.Key));

            await Task.Delay(3000);

            //var d30 = dinos.Subscribe(d => Console.WriteLine("D30: " + d.Key));

            await Task.Delay(3000);


            d1.Dispose();

            await Task.Delay(3000);

            d2.Dispose();
            //d30.Dispose();
            await Task.Delay(3000);

            var d3 = firebase.Child("dinosaurs").AsRealtimeDatabase<Dinosaur>("a").AsObservable().Subscribe(d => Console.WriteLine("D3: " + d.Key));

            await Task.Delay(3000);

            var d4 = dinos.Subscribe(d => Console.WriteLine("D4: " + d.Key));
            var d5 = dinos.Subscribe(d => Console.WriteLine("D5: " + d.Key));

            dinos.Wait();
        }
    }
}
