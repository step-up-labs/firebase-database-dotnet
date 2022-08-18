namespace FireBase.Database.Tests
{
    using System.Threading.Tasks;

    using Firebase.Database;
    using Firebase.Database.Query;
    using FluentAssertions;
    using Xunit;

    public class FirebasePathTests
    {
        public const string BasePath = "http://base.path.net";
        public const string Token = "aBcEfgH";

        [Fact]
        public void TestAuthPath()
        {
            var client = new FirebaseClient(BasePath, new FirebaseOptions());

            var path = client.Child("resource").WithAuth(Token).BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json?auth={Token}");
        }

        [Fact]
        public void TestNestedAuthPath()
        {
            var client = new FirebaseClient(BasePath, new FirebaseOptions());

            var path = client.Child("resource").OrderByKey().WithAuth(Token).BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json?orderBy=\"$key\"&auth={Token}");
        }

        [Fact]
        public void TestClientAuthPath()
        {
            var client = new FirebaseClient(BasePath, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(Token) });

            var path = client.Child("resource").OrderByKey().BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json?orderBy=\"$key\"&auth={Token}");
        }

        [Fact]
        public void TestCaseSensitivePath()
        {
            var client = new FirebaseClient(BasePath, new FirebaseOptions());

            var path = client.Child("resource").OrderByKey().StartAt(Token).EndAt(Token).BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json?orderBy=\"$key\"&startAt=\"{Token}\"&endAt=\"{Token}\"");
        }

        [Fact]
        public void OnlyChildPath()
        {
            var client = new FirebaseClient(BasePath, new FirebaseOptions());

            var path = client.Child("resource").BuildUrlAsync().Result;

            path.Should().Be($"{BasePath}/resource/.json");
        }
    }
}
