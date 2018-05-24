using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Offline;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamarinForms.Services
{
    public class FirebaseOfflineDataStore<T> : IDataStore<T>
        where T : class
    {
        private const string BaseUrl = "https://database-test-db252.firebaseio.com";

        private readonly RealtimeDatabase<T> _realtimeDb;

        public FirebaseOfflineDataStore(FirebaseAuthService authService, string path, string key = "")
        {
            FirebaseOptions options = new FirebaseOptions()
            {
                OfflineDatabaseFactory = (t, s) => new OfflineDatabase(t, s),
                AuthTokenAsyncFactory = async () => await authService.GetFirebaseAuthToken()
            };

            // The offline database filename is named after type T.
            // So, if you have more than one list of type T objects, you need to differentiate it
            // by adding a filename modifier; which is what we're using the "key" parameter for.
            var client = new FirebaseClient(BaseUrl, options);
            _realtimeDb = client
                .Child(path)
                .AsRealtimeDatabase<T>(key, "", StreamingOptions.LatestOnly, InitialPullStrategy.MissingOnly, true);
        }

        public async Task<bool> AddItemAsync(T item)
        {
            try
            {
                string key = _realtimeDb.Post(item);
            }
            catch(Exception ex)
            {
                return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(string id, T item)
        {
            try
            {
                _realtimeDb.Put(id, item);
            }
            catch(Exception ex)
            {
                return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            try
            {
                _realtimeDb.Delete(id);
            }
            catch(Exception ex)
            {
                return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }

        public async Task<T> GetItemAsync(string id)
        {
            if(_realtimeDb.Database?.Count == 0)
            {
                try
                {
                    await _realtimeDb.PullAsync();
                }
                catch(Exception ex)
                {
                    return null;
                }
            }

            bool success = _realtimeDb.Database.TryGetValue(id, out OfflineEntry offlineEntry);

            var result = offlineEntry?.Deserialize<T>();

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false)
        {
            if(_realtimeDb.Database?.Count == 0)
            {
                try
                {
                    await _realtimeDb.PullAsync();
                }
                catch(Exception ex)
                {
                    return null;
                }
            }

            var result = _realtimeDb
                .Once()
                .Select(x => x.Object);

            return await Task.FromResult(result);
        }
    }
}