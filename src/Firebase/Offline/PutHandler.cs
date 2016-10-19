namespace Firebase.Database.Offline
{
    using Firebase.Database.Query;

    using System.Threading.Tasks;

    public class PutHandler<T> : IPutHandler<T>
    {
        public Task PutAsync(ChildQuery query, string key, OfflineEntry entry)
        {
            return query.Child(key).PutAsync(entry.Deserialize<T>());
        }
    }
}
