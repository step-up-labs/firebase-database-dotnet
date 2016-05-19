namespace Firebase.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using System.Collections;
    public static class HttpClientExtensions
    {
        public static async Task<T> GetObjectAsync<T>(this HttpClient client, string requestUri)
        {
            var data = await client.GetStringAsync(requestUri);
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static async Task<IReadOnlyCollection<FirebaseObject<T>>> GetObjectCollectionAsync<T>(this HttpClient client, string requestUri)
        {
            var result = await client.GetObjectAsync<Dictionary<string, object>>(requestUri);

            return result
                .Select(s => new FirebaseObject<T>(s.Key, JsonConvert.DeserializeObject<T>(s.Value.ToString())))
                .ToList();
        }

        public static IReadOnlyCollection<FirebaseObject<T>> GetObjectCollection<T>(this string data)
        {
            var items = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

            return items
                .Select(s => new FirebaseObject<T>(s.Key, JsonConvert.DeserializeObject<T>(s.Value.ToString())))
                .ToList();
        }

        public static IEnumerable<FirebaseObject<object>> GetObjectCollection(this string data, Type elementType)
        {
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), elementType);
            var dictionary = JsonConvert.DeserializeObject(data, dictionaryType) as IDictionary;

            foreach (DictionaryEntry item in dictionary)
            {
                yield return new FirebaseObject<object>((string)item.Key, item.Value);
            }
        }
    }
}
