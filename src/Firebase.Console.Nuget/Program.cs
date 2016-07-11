using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firebase.Console.Nuget
{
    using Firebase.Database;

    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        private static async Task RunAsync()
        {
            var firebase = new FirebaseClient("https://torrid-inferno-3642.firebaseio.com/");

            var dino = await firebase.Child("products").PostAsync(new Product { Id = "1", Name = "Item 1" });


        }
    }
}
