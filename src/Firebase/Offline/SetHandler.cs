namespace Firebase.Database.Offline
{
    using Firebase.Database.Query;

    using System.Threading.Tasks;

    public class SetHandler<T> : ISetHandler<T>
    {
        public virtual Task SetAsync(ChildQuery query, string key, OfflineEntry entry)
        {
            using (var child = query.Child(key))
            {
                if (entry.SyncOptions == SyncOptions.Put)
                {
                    return child.PutAsync(entry.Data);
                }
                else
                {
                    
                    return child.PatchAsync(entry.Data);
                }
            }
        }
    }
}
