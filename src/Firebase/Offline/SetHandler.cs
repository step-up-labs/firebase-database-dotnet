namespace Firebase.Database.Offline
{
    using Firebase.Database.Query;

    using System.Threading.Tasks;

    public class SetHandler<T> : ISetHandler<T>
    {
        public virtual Task SetAsync(ChildQuery query, string key, OfflineEntry entry)
        {
            if (entry.SyncOptions == SyncOptions.Put)
            {
                return query.Child(key).PutAsync(entry.Data);
            }
            else
            {
                return query.Child(key).PatchAsync(entry.Data);
            }
        }
    }
}
