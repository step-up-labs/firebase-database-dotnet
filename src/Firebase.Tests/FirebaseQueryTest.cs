using Firebase.Database.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firebase.Database.Tests
{
    [TestClass]
    public class FirebaseQueryTest
    {
        public const string BasePath = "https://fir-testproject-d6ced.firebaseio.com/";
        [TestMethod]
        public void TestPostAsyncParameterWithAllNull()
        {
            FirebaseClient firebaseClient = new FirebaseClient(BasePath, new FirebaseOptions());
            FirebaseObject<string> result = firebaseClient.Child("dinosaurs").PostAsync("", "").Result;

            Assert.AreEqual(null, result.Key);
        }
        [TestMethod]
        public void TestPostAsyncParameterWithDataNull()
        {
            string generatedKey = Guid.NewGuid().ToString();
            FirebaseClient firebaseClient = new FirebaseClient(BasePath, new FirebaseOptions());
            FirebaseObject<string> result = firebaseClient.Child("dinosaurs").PostAsync("", generatedKey).Result;

            Assert.AreEqual(null, result.Key);
        }
        [TestMethod]
        public void TestPostAsyncParameterWithGeneratedKeyNull()
        {
            FirebaseClient firebaseClient = new FirebaseClient(BasePath, new FirebaseOptions());
            string data = JsonConvert.SerializeObject(new Entities.Dinosaur(2, 2, 2));
            FirebaseObject<string> result = firebaseClient.Child("dinosaurs").PostAsync(data, "").Result;

            Assert.AreEqual(null, result.Key);
        }
        [TestMethod]
        public void TestPostAsyncParameterWithAllParameterPassed()
        {
            FirebaseClient firebaseClient = new FirebaseClient(BasePath, new FirebaseOptions());
            string generatedKey = Guid.NewGuid().ToString();
            string data = JsonConvert.SerializeObject(new Entities.Dinosaur(2, 2, 2));
            FirebaseObject<string> result = firebaseClient.Child("dinosaurs").PostAsync(data, generatedKey).Result;

            Assert.IsNotNull(result.Key, "the object returned is null");
        }

    }
}
