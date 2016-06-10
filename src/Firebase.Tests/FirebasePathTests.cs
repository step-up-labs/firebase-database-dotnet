namespace FireBase.Database.Tests
{
    using System.Threading.Tasks;

    using Firebase.Database;
    using Firebase.Database.Query;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FirebasePathTests
    {
        public const string BasePath = "http://base.path.net";
        public const string Token = "aBcEfgH";

        [TestMethod]
        public void TestAuthPath()
        {
            var client = new FirebaseClient(BasePath);

            var path = client.Child("resource").WithAuth(Token).BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json?auth={Token}");
        }

        [TestMethod]
        public void TestNestedAuthPath()
        {
            var client = new FirebaseClient(BasePath);

            var path = client.Child("resource").OrderByKey().WithAuth(Token).BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json?orderBy=\"$key\"&auth={Token}");
        }

        [TestMethod]
        public void TestClientAuthPath()
        {
            var client = new FirebaseClient(BasePath, () => Task.FromResult(Token));

            var path = client.Child("resource").OrderByKey().BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json?orderBy=\"$key\"&auth={Token}");
        }

        [TestMethod]
        public void TestCaseSensitivePath()
        {
            var client = new FirebaseClient(BasePath);

            var path = client.Child("resource").OrderByKey().StartAt(Token).EndAt(Token).BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json?orderBy=\"$key\"&startAt=\"{Token}\"&endAt=\"{Token}\"");
        }
    }
}
