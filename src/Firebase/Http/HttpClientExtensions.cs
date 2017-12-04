namespace Firebase.Database.Http
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using System.Net;

    /// <summary>
    /// The http client extensions for object deserializations.
    /// </summary>
    internal static class HttpClientExtensions
    {
        /// <summary>
        /// The get object collection async.
        /// </summary>
        /// <param name="client"> The client. </param>
        /// <param name="requestUri"> The request uri. </param>  
        /// <param name="jsonSerializerSettings"> The specific JSON Serializer Settings. </param>  
        /// <typeparam name="T"> The type of entities the collection should contain. </typeparam>
        /// <returns> The <see cref="Task"/>. </returns>
        public static async Task<IReadOnlyCollection<FirebaseObject<T>>> GetObjectCollectionAsync<T>(this HttpClient client, string requestUri,
            JsonSerializerSettings jsonSerializerSettings)
        {
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                var response = await client.GetAsync(requestUri).ConfigureAwait(false);
                statusCode = response.StatusCode;
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, T>>(responseData, jsonSerializerSettings);

                if (dictionary == null)
                {
                    return new FirebaseObject<T>[0];
                }

                return dictionary.Select(item => new FirebaseObject<T>(item.Key, item.Value)).ToList();
            }
            catch (Exception ex)
            {
                throw new FirebaseException(requestUri, string.Empty, responseData, statusCode, ex);
            }
        }

        /// <summary>
        /// The get object collection async.
        /// </summary>
        /// <param name="data"> The json data. </param>
        /// <param name="elementType"> The type of entities the collection should contain. </param>
        /// <returns> The <see cref="Task"/>.  </returns>
        public static IEnumerable<FirebaseObject<object>> GetObjectCollection(this string data, Type elementType)
        {
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), elementType);
            IDictionary dictionary = null;

            if (data.StartsWith("["))
            {
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = JsonConvert.DeserializeObject(data, listType) as IList;
                dictionary = Activator.CreateInstance(dictionaryType) as IDictionary;
                var index = 0;
                foreach (var item in list) dictionary.Add(index++.ToString(), item);
            }
            else
            {
                dictionary = JsonConvert.DeserializeObject(data, dictionaryType) as IDictionary;
            }

            if (dictionary == null)
            {
                yield break;
            }

            foreach (DictionaryEntry item in dictionary)
            {
                yield return new FirebaseObject<object>((string)item.Key, item.Value);
            }
        }
    }
}
